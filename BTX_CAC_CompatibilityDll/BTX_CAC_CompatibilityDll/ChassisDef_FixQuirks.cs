using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using HarmonyLib;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(ChassisDef), "FromJSON")]
    class ChassisDef_FromJson
    {
        public static void Postfix(ChassisDef __instance)
        {
            if (__instance.ChassisTags.Contains("mech_quirk_improvedsensorsbap"))
            {
                __instance.ChassisTags.Remove("mech_quirk_improvedsensorsbap");
                __instance.ChassisTags.Add("mech_quirk_improvedsensors");
            }
        }
    }

    class QuirkToolTips_DetailMechQuirks
    {
        public static void Postfix(ref string __result)
        {
            __result = __result.Replace("Can see through a single ECM Ghost Effect", "Can spot targets covered by ECM for indirect fire")
                .Replace("Perform a Sensor Lock on all short range enemies, longer cooldown and shorter range than an Active Probe", "Increased sensor range");
        }
    }
}
