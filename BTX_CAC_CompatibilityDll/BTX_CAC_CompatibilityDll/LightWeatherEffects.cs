using AccessExtension;
using BattleTech;
using CustAmmoCategories;
using Extended_CE;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    public class LightWeatherEffects
    {
        internal static float Light_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            return target.StatCollection.GetValue<float>("LightAccuracy");
        }

        internal static float Illuminated_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            if (AEPStatic.GetExtendedCE_Core_CurrentLight() == LightValue.DawnDusk)
            {
                if (target is Mech m && m.CurrentHeat > 75)
                    return -1;
            }
            else if (AEPStatic.GetExtendedCE_Core_CurrentLight() == LightValue.Night)
            {
                if (target is Mech m)
                {
                    if (m.CurrentHeat >= 120)
                        return -2;
                    if (m.CurrentHeat >= 60)
                        return -1;
                }
            }
            return 0;
        }

        internal static float Tracer_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            if (wep.ammo().DamagePerShot < 0 && AEPStatic.GetExtendedCE_Core_CurrentLight() > LightValue.Day)
                return -1;
            return 0;
        }
    }

    [HarmonyPatch]
    public class BEXStatusEffects_XLightAccuracyEffect
    {
        //[HarmonyPatch("Extended_CE.BEXStatusEffects", "LowLightAccuracyEffect", MethodType.Getter)]
        //[HarmonyPatch("Extended_CE.BEXStatusEffects", "NoLightAccuracyEffect", MethodType.Getter)]
        public static void Postfix(EffectData __result)
        {
            __result.statisticData.statName = "LightAccuracy";
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            Assembly a = AccessExtensionPatcher.GetLoadedAssemblyByName("Extended_CE");
            if (a != null)
            {
                Type t = a.GetType("Extended_CE.BEXStatusEffects");
                yield return t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where((x) => x.Name == "LowLightAccuracyEffect").Single().GetMethod;
                yield return t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where((x) => x.Name == "NoLightAccuracyEffect").Single().GetMethod;
            }
        }
    }

}
