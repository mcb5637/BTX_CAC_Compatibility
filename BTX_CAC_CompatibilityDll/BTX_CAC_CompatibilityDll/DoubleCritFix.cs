using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using System.Diagnostics;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(MechComponent), "DamageComponent")]
    internal class MechComponent_DamageComponent
    {
        [HarmonyAfter("BEX.BattleTech.Extended_CE")]
        public static bool Prefix(MechComponent __instance, ComponentDamageLevel damageLevel)
        {
            if (__instance.DamageLevel == damageLevel || __instance.DamageLevel == ComponentDamageLevel.Destroyed)
                return false;
            return true;
        }
    }
}
