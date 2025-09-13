using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using BattleTech;
using BattleTech.Framework;
using System.Reflection.Emit;
using BattleTech.UI;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch]
    public static class ContractOverride_FixMaxPlayers
    {
        [HarmonyPatch(typeof(ContractOverride), "FromJSONFull")]
        [HarmonyPatch(typeof(ContractOverride), "FullRehydrate")]
        [HarmonyPostfix]
        public static void Postfix(ContractOverride __instance)
        {
            if (Main.Sett.Use4LimitOnAllStoryMissions && __instance.IsAnyStoryContract())
                return;

            if (__instance.maxNumberOfPlayerUnits == 4 && !__instance.IsContractLimitedTo4Units())
            {
                __instance.maxNumberOfPlayerUnits = Main.Sett.MaxNumberOfPlayerUnitsOverride;
            }
        }

        private static bool IsAnyStoryContract(this ContractOverride contractOverride) =>
            contractOverride.contractDisplayStyle == ContractDisplayStyle.BaseCampaignStory || contractOverride.contractDisplayStyle == ContractDisplayStyle.BaseCampaignRestoration;

        private static bool IsContractLimitedTo4Units(this ContractOverride contractOverride) =>
            Main.Sett.Use4LimitOnContractIds.Contains(contractOverride.ID);
    }
}
