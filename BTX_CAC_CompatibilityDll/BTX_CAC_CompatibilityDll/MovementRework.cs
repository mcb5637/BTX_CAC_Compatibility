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
    internal enum SelfMovedModifier : int
    {
        None,
        Vehicle,
        Walk,
        Run,
        Sprint,
        Jump,
    }
    [HarmonyPatch]
    internal class MovementRework
    {
        private const string LastTurnMoveStat = "CAC_C_Last_Turn_Move";
        private const string LastTurnMoveTypeStat = "CAC_C_Last_Turn_MoveType";
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

        private static float MovedSelfModifier(AbstractActor attacker, SelfMovedModifier mod)
        {
            float r = 0;
            switch (mod)
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
                    r = AEPStatic.GetCESettings().RunningToHitCumulativePenalty;
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
                    r = AEPStatic.GetCESettings().ToHitSelfJumped;
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

        internal static bool MovedSelfUseThisTurn(AbstractActor attacker, Vector3 apos)
        {
            if (attacker.CanMoveAfterShooting)
                return attacker.HasMovedThisRound || (attacker.CurrentPosition - apos).magnitude > 0.001f;
            else
                return true;
        }

        internal static float MovedSelfModifier_Fallback(AbstractActor attacker, Vector3 apos)
        {
            if (MovedSelfUseThisTurn(attacker, apos))
                return MovedSelfModifier(attacker, ClassifyMoved(attacker, apos));
            else
                return attacker.StatCollection.GetStatistic(LastTurnMoveStat)?.CurrentValue?.Value<float>() ?? 0.0f;
        }

        internal static float MovedSelf_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            return MovedSelfModifier_Fallback(attacker, apos);
        }

        internal static string MovedSelf_EffectName(ToHit h, AbstractActor a, Weapon w, ICombatant t, Vector3 ap, Vector3 tp, LineOfFireLevel lof, MeleeAttackType mt, bool cs)
        {
            if (MovedSelfUseThisTurn(a, ap))
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
            else
            {
                SelfMovedModifier m = (SelfMovedModifier)(a.StatCollection.GetStatistic(LastTurnMoveTypeStat)?.CurrentValue?.Value<int>() ?? (int)SelfMovedModifier.None);
                switch (m)
                {
                    case SelfMovedModifier.Walk:
                        return "WALKED (LT)";
                    case SelfMovedModifier.Run:
                        return "RAN (LT)";
                    case SelfMovedModifier.Sprint:
                        return "SPRINTED (LT)";
                    case SelfMovedModifier.Jump:
                        return "JUMPED (LT)";
                    default:
                        return "MOVED (LT)";
                }
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


        [HarmonyPatch(typeof(Mech), nameof(Mech.CanSprint), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Mech_CanSprint(Mech __instance, ref bool __result)
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

        [HarmonyPatch(typeof(Mech), nameof(Mech.OnActivationEnd))]
        [HarmonyPostfix]
        public static void Mech_OnActivationEnd(Mech __instance)
        {
            SelfMovedModifier mt = ClassifyMoved(__instance, __instance.CurrentPosition);
            float m = MovedSelfModifier(__instance, mt);
            __instance.StatCollection.GetOrCreateStatisic(LastTurnMoveTypeStat, 0).SetValue((int)mt);
            __instance.StatCollection.GetOrCreateStatisic(LastTurnMoveStat, 0.0f).SetValue(m);
        }
    }
}
