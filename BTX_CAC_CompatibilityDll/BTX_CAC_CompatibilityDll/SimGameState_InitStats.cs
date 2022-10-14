using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BiggerDrops.Features;

namespace BTX_CAC_CompatibilityDll
{
    internal class SimGameState_InitStats
    {
        public static void Postfix(SimGameState __instance)
        {
            if (!__instance.CompanyStats.ContainsStatistic("BiggerDrops_BaseMechSlots") || __instance.CompanyStats.GetValue<int>("BiggerDrops_BaseMechSlots") == 0)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_BaseMechSlots");
                __instance.CompanyStats.AddStatistic("BiggerDrops_BaseMechSlots", 4);
                if (__instance.CompanyStats.GetValue<int>("BiggerDrops_AdditionalMechSlots") > 4)
                    __instance.CompanyStats.Int_Add(__instance.CompanyStats.GetStatistic("BiggerDrops_AdditionalMechSlots"), -4);
                DropManager.UpdateCULances();
            }
        }

        public static void Patch(HarmonyInstance h)
        {
            HarmonyMethod m = new HarmonyMethod(AccessTools.DeclaredMethod(typeof(SimGameState_InitStats), "Postfix"))
            {
                after = new string[] { "de.morphyum.BiggerDrops" }
            };
            h.Patch(AccessTools.DeclaredMethod(typeof(SimGameState), "Rehydrate"), null, m, null);
            h.Patch(AccessTools.DeclaredMethod(typeof(SimGameState), "InitCompanyStats"), null, m, null);
        }
    }
}
