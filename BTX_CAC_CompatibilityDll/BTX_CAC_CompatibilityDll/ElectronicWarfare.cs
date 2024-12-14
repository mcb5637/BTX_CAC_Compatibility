using BattleTech;
using CustAmmoCategories;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TScript.Ops;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch]
    internal class ElectronicWarfare
    {
        internal static float ECM_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            float mod = 0;
            if (target is AbstractActor at && at.StatCollection.GetValue<float>("DefendedByECM") > 0.0f)
            {
                mod += 4;
            }
            return mod;
        }

        internal static bool IsClan(Weapon w)
        {
            return w.weaponDef.ComponentTags.Contains("component_type_clan");
        }
        internal static bool DoesNARCApply(Weapon wep, ICombatant t)
        {
            if (!wep.weaponDef.ComponentTags.Contains("component_usesnarc"))
                return false;
            if (wep.Unguided())
                return false;
            if (CustomClustering.IsArtemisMode(wep))
                return false;
            return t.StatCollection.GetValue<float>("NARCCount") > 0.0f && t.StatCollection.GetValue<float>("DefendedByECM") <= 0.0f;
        }
        internal static bool DoesTAGApply(Weapon wep, ICombatant t)
        {
            if (!wep.weaponDef.ComponentTags.Contains("component_usestag"))
                return false;
            if (wep.Unguided())
                return false;
            if (CustomClustering.IsArtemisMode(wep))
                return false;
            return t.StatCollection.GetValue<float>(IsClan(wep) ? "TAGCountClan" : "TAGCount") > 0.0f;
        }
        private static float GetTAGBonus(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            if (!DoesTAGApply(wep, target))
                return 0.0f;
            return -ToHitModifiersHelper.GetIndirectModifier(tohit, attacker, wep, target, apos, tpos, lof, mat, calledshot) - ToHitModifiersHelper.GetTargetSpeedModifier(tohit, attacker, wep, target, apos, tpos, lof, mat, calledshot);
        }
        private static float GetNARCBonus(Weapon wep, ICombatant target)
        {
            return DoesNARCApply(wep, target) ? -4.0f : 0.0f;
        }
        internal static float NARC_TAG_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            return Mathf.Min(GetNARCBonus(wep, target), GetTAGBonus(tohit, attacker, wep, target, apos, tpos, lof, mat, calledshot));
        }
        internal static string NARC_TAG_EffectName(ToHit h, AbstractActor a, Weapon w, ICombatant t, Vector3 ap, Vector3 tp, LineOfFireLevel lof, MeleeAttackType mt, bool cs)
        {
            if (GetNARCBonus(w, t) <= GetTAGBonus(h, a, w, t, ap, tp, lof, mt, cs))
                return "NARC";
            else
                return "TAG";
        }

        [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.HasIndirectFireImmunity), MethodType.Getter)]
        [HarmonyPrefix]
        public static void AbstractActor_HasIndirectFireImmunity_Postfix(AbstractActor __instance, ref bool __result)
        {
            if (__instance.StatCollection.GetValue<float>("IndirectImmuneFloat") > 0)
                __result = true;
        }

        [HarmonyPatch(typeof(Effect))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(string), typeof(int), typeof(object), typeof(object), typeof(string), typeof(string), typeof(EffectData), typeof(WeaponHitInfo), typeof(int) })]
        [HarmonyPostfix]
        public static void Effect_ctor_Postfix(Effect __instance, EffectData effectData, string targetID)
        {
            if (effectData.durationData.ticksOnMovements && effectData.durationData.useActivationsOfTarget)
            {
                __instance.Duration.activationActorGUID = targetID;
            }
        }

        [HarmonyPatch(typeof(LineOfSight), "GetLineOfFireUncached")]
        [HarmonyPostfix]
        public static void LineOfSight_GetLineOfFireUncached_Postfix(LineOfSight __instance, AbstractActor source, Vector3 sourcePosition, ICombatant target, ref LineOfFireLevel __result)
        {
            if (target is AbstractActor a && a.HasIndirectFireImmunity && __instance.GetVisibilityToTargetWithPositionsAndRotations(source, sourcePosition, a) != VisibilityLevel.LOSFull)
                __result = LineOfFireLevel.LOFBlocked;
        }
    }
}
