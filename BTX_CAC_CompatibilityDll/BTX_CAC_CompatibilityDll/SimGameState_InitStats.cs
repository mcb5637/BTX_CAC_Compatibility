using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BiggerDrops.Features;

namespace BTX_CAC_CompatibilityDll
{
    internal class SimGameState_InitStats
    {
        public static int GetUpgradeStat(SimGameState s, string stat)
        {
            int r = 0;
            foreach (var u in s.ShipUpgrades)
            {
                if (s.HasShipUpgrade(u.Description.Id))
                {
                    foreach (var st in u.Stats)
                    {
                        if (st.name == stat)
                        {
                            r += st.ToInt();
                        }
                    }
                }
            }
            return r;
        }

        public static void Postfix(SimGameState __instance)
        {
            int updated = 0;
            if (__instance.CompanyStats.GetValue<int>("BiggerDrops_BaseMechSlots") != 4)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_BaseMechSlots");
                __instance.CompanyStats.AddStatistic("BiggerDrops_BaseMechSlots", 4);
                updated++;
            }
            int u = GetUpgradeStat(__instance, "BiggerDrops_AdditionalMechSlots");
            if (__instance.CompanyStats.GetValue<int>("BiggerDrops_AdditionalMechSlots") != u)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_AdditionalMechSlots");
                __instance.CompanyStats.AddStatistic("BiggerDrops_AdditionalMechSlots", u);
                updated++;
            }
            int u2 = GetUpgradeStat(__instance, "BiggerDrops_HotDropMechSlots");
            if (__instance.CompanyStats.GetValue<int>("BiggerDrops_HotDropMechSlots") != u2)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_HotDropMechSlots");
                __instance.CompanyStats.AddStatistic("BiggerDrops_HotDropMechSlots", u2);
                updated++;
            }
            int u3 = BiggerDrops.BiggerDrops.settings.defaultMaxTonnage + GetUpgradeStat(__instance, "BiggerDrops_MaxTonnage");
            if (__instance.CompanyStats.GetValue<int>("BiggerDrops_MaxTonnage") != u3)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_MaxTonnage");
                __instance.CompanyStats.AddStatistic("BiggerDrops_MaxTonnage", u3);
                updated++;
            }
            if (updated >=1) DropManager.UpdateCULances();
            Main.Log.Log($"Dropslot stats: " +
                $"BiggerDrops_BaseMechSlots: {__instance.CompanyStats.GetValue<int>("BiggerDrops_BaseMechSlots")}, " +
                $"BiggerDrops_AdditionalMechSlots: {__instance.CompanyStats.GetValue<int>("BiggerDrops_AdditionalMechSlots")}, " +
                $"BiggerDrops_HotDropMechSlots: {__instance.CompanyStats.GetValue<int>("BiggerDrops_HotDropMechSlots")}, " +
                $"BiggerDrops_MaxTonnage: {__instance.CompanyStats.GetValue<int>("BiggerDrops_MaxTonnage")}"
            );
        }

        public static void Patch(Harmony h)
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
