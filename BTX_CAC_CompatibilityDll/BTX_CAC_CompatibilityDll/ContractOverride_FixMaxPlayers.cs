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
    [HarmonyPatch(typeof(ContractOverride), "FromJSONFull")]
    [HarmonyPatch(typeof(ContractOverride), "FullRehydrate")]
    public class ContractOverride_FixMaxPlayers
    {
        public static void Postfix(ContractOverride __instance)
        {
            if (Main.Sett.Use4LimitOnAllStoryMissions && IsAnyStoryContract(__instance))
                return;

            if (__instance.maxNumberOfPlayerUnits == 4 && !IsContractLimitedTo4Units(__instance))
            {
                __instance.maxNumberOfPlayerUnits = 12;
            }
        }

        private static bool IsAnyStoryContract(ContractOverride contractOverride) =>
            contractOverride.contractDisplayStyle is ContractDisplayStyle.BaseCampaignStory or ContractDisplayStyle.BaseCampaignRestoration;

        private static bool IsContractLimitedTo4Units(ContractOverride contractOverride) =>
            Main.Sett.Use4LimitOnContractIds.Contains(contractOverride.ID);
    }
}
