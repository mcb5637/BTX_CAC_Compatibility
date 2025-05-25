using BattleTech.UI;
using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CustAmmoCategories;
using Extended_CE;
using HarmonyLib;

namespace BTX_CAC_CompatibilityDll
{
    internal enum SelfMovedModifier
    {
        None,
        Vehicle,
        Walk,
        Run,
        Sprint,
        Jump,
    }
    internal class MovementRework
    {
        private static bool IsRunning(float rem, float max)
        {
            return rem < max / 3;
        }

        internal static SelfMovedModifier ClassifyMoved(AbstractActor a, Vector3 apos)
        {
            if (a.StatCollection.GetValue<bool>("IgnoreHeatMovementPenalties"))
                return SelfMovedModifier.None;
            if (a is Mech m)
            {
                if (m is CustomUnits.FakeVehicleMech)
                {
                    if (m.HasMovedThisRound)
                        return SelfMovedModifier.Vehicle;
                    else
                        return SelfMovedModifier.None;
                }
                else
                {
                    if (m.HasMovedThisRound)
                    {
                        if (m.JumpedLastRound)
                            return SelfMovedModifier.Jump;
                        if (m.SprintedLastRound)
                            return SelfMovedModifier.Sprint;
                        float max = m.MaxMoveDistance();
                        float rem = m.MoveCostLeft();
                        if (IsRunning(rem, max))
                            return SelfMovedModifier.Run;
                        return SelfMovedModifier.Walk;
                    }
                    else if ((a.CurrentPosition - apos).magnitude > 0.001f)
                    {
                        if (m.JumpPathing != null && m.JumpPathing.IsLockedToDest)
                            return SelfMovedModifier.Jump;
                        if (m.Pathing != null && m.Pathing.HasPath)
                        {
                            if (m.Pathing.MoveType == MoveType.Sprinting)
                                return SelfMovedModifier.Sprint;
                            if (m.Pathing.MoveType == MoveType.Walking || m.Pathing.MoveType == MoveType.Melee)
                            {
                                float max = m.MaxMoveDistance();
                                float rem = m.MoveCostLeft();
                                if (IsRunning(rem, max))
                                    return SelfMovedModifier.Run;
                                return SelfMovedModifier.Walk;
                            }
                        }
                    }
                }
            }
            return SelfMovedModifier.None;
        }

        internal static float MovedSelf_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            float r = 0;
            switch (ClassifyMoved(attacker, apos))
            {
                case SelfMovedModifier.None:
                    break;
                case SelfMovedModifier.Vehicle:
                    r = attacker.Combat.Constants.ToHit.ToHitSelfWalkVehicle;
                    break;
                case SelfMovedModifier.Sprint:
                    r = attacker.Combat.Constants.ToHit.ToHitSelfSprinted;
                    goto case SelfMovedModifier.Walk;
                case SelfMovedModifier.Run:
                    r = 2;
                    goto case SelfMovedModifier.Walk;
                case SelfMovedModifier.Walk:
                    switch (((Mech)attacker).weightClass)
                    {
                        case WeightClass.ASSAULT:
                            r += attacker.Combat.Constants.ToHit.ToHitSelfWalkAssault;
                            break;
                        case WeightClass.HEAVY:
                            r += attacker.Combat.Constants.ToHit.ToHitSelfWalkHeavy;
                            break;
                        case WeightClass.MEDIUM:
                            r += attacker.Combat.Constants.ToHit.ToHitSelfWalkMedium;
                            break;
                        case WeightClass.LIGHT:
                            r += attacker.Combat.Constants.ToHit.ToHitSelfWalkLight;
                            break;
                    }
                    break;
                case SelfMovedModifier.Jump:
                    r = 3;
                    break;
            }
            if (r >= 1f && attacker.StatCollection.GetValue<bool>("CanUseFocusedBalance"))
            {
                Pilot p = attacker.GetPilot();
                if (p != null && p.Abilities.Any((a) => a.Def.Description.Id == "AbilityDefP8"))
                    r -= 1;
            }
            return r;
        }

        internal static string MovedSelf_EffectName(ToHit h, AbstractActor a, Weapon w, ICombatant t, Vector3 ap, Vector3 tp, LineOfFireLevel lof, MeleeAttackType mt, bool cs)
        {
            switch (ClassifyMoved(a, ap))
            {
                case SelfMovedModifier.Walk:
                    return "WALKED";
                case SelfMovedModifier.Run:
                    return "RAN";
                case SelfMovedModifier.Sprint:
                    return "SPRINTED";
                case SelfMovedModifier.Jump:
                    return "JUMPED";
                default:
                    return "MOVED";
            }
        }

        internal static string MoveTypeDisplayOverride(CombatHUD h, AbstractActor a, float max, float rem, string prev)
        {
            if (prev == "MOVE")
            {
                return IsRunning(rem, max) ? "RUN" : "WALK";
            }
            return prev;
        }
    }

    [HarmonyPatch(typeof(Mech), nameof(Mech.CanSprint), MethodType.Getter)]
    public static class Mech_CanSprint
    {
        public static void Postfix(Mech __instance, ref bool __result)
        {
            if (__instance is CustomUnits.FakeVehicleMech)
            {
                __result = false;
                return;
            }
            if (BTComponents.MechTTRuleInfo.MechTTStatStore.ContainsKey(__instance.uid))
            {
                __result &= BTComponents.MechTTRuleInfo.MechTTStatStore[__instance.uid].HipCrits == 0;
            }
        }
    }
}
