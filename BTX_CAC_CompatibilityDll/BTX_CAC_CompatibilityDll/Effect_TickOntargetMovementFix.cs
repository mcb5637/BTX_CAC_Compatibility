using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using HarmonyLib;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Effect))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(string), typeof(int), typeof(object), typeof(object), typeof(string), typeof(string), typeof(EffectData), typeof(WeaponHitInfo), typeof(int) })]
    class Effect_Ctor
    {
        public static void Postfix(Effect __instance, EffectData effectData, string targetID)
        {
            if (effectData.durationData.ticksOnMovements && effectData.durationData.useActivationsOfTarget)
            {
                __instance.Duration.activationActorGUID = targetID;
            }
        }
    }
}
