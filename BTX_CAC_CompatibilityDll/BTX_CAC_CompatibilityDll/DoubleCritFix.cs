using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BattleTech;
using System.Diagnostics;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Mech), "GetComponentInSlot")]
    internal class Mech_GetComponentInSlot
    {
        [HarmonyAfter("BEX.BattleTech.Extended_CE")]
        public static void Postfix(ref MechComponent __result)
        {
            if (AEPStatic.GetExtendedCE_BtComponents_CheckingForCrit() && __result != null && __result.DamageLevel == ComponentDamageLevel.Destroyed)
                __result = null;
        }
    }
}
