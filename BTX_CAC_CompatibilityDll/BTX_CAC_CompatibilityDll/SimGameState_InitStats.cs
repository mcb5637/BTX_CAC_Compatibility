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
            bool update = false;
            if (__instance.CompanyStats.GetValue<int>("BiggerDrops_BaseMechSlots") != 4)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_BaseMechSlots");
                __instance.CompanyStats.AddStatistic("BiggerDrops_BaseMechSlots", 4);
                update = true;
            }
            int u = GetUpgradeStat(__instance, "BiggerDrops_AdditionalMechSlots");
            if (__instance.CompanyStats.GetValue<int>("BiggerDrops_AdditionalMechSlots") != u)
            {
                __instance.CompanyStats.RemoveStatistic("BiggerDrops_AdditionalMechSlots");
                __instance.CompanyStats.AddStatistic("BiggerDrops_AdditionalMechSlots", u);
                update = true;
            }
            if (update)
                DropManager.UpdateCULances();
            Main.Log.Log($"dropslot stats: BiggerDrops_BaseMechSlots: {__instance.CompanyStats.GetValue<int>("BiggerDrops_BaseMechSlots")}, BiggerDrops_AdditionalMechSlots: {__instance.CompanyStats.GetValue<int>("BiggerDrops_AdditionalMechSlots")}, Upgrades: {u}");
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
