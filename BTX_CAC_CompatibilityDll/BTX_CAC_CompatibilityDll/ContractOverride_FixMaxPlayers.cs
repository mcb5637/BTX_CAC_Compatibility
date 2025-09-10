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

    [HarmonyPatch(typeof(LanceConfiguratorPanel), "LoadLanceConfiguration")]
    public static class LanceConfiguratorPanel_LoadLanceConfiguration
    {
        [HarmonyPrepare]
        public static bool Prepare() => Main.Sett.EnforceContractLimits;

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(LanceConfiguratorPanel __instance)
        {
            var loadoutSlotsField = typeof(LanceConfiguratorPanel).GetField("loadoutSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            if (loadoutSlotsField == null) return;

            var loadoutSlots = (LanceLoadoutSlot[])loadoutSlotsField.GetValue(__instance);
            if (loadoutSlots != null)
            {
                for (int i = 0; i < loadoutSlots.Length; i++)
                {
                    bool locked = i >= __instance.activeContract.Override.maxNumberOfPlayerUnits;
                    loadoutSlots[i].SetLockState(locked ?
                        LanceLoadoutSlot.LockState.Full :
                        LanceLoadoutSlot.LockState.Unlocked);
                }
            }
        }
    }
}
