using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustAmmoCategories;
using HarmonyLib;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(AdvWeaponHitInfo), "ApplyHitEffects")]
    class AdvWeaponHitInfo_ApplyHitEffects
    {
        public static void Postfix(AdvWeaponHitInfo __instance)
        {
            foreach (AdvWeaponResolveInfo ri in __instance.resolveInfo.Values)
            {
                ri.floatieMessages = ri.floatieMessages.Where((s) => !s.EndsWith("VFX")).ToList();
            }
        }
    }
}
