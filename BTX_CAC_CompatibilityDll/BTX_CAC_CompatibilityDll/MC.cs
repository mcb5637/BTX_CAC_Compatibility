using Harmony;
using MissionControl.Rules;
using System.Reflection;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(MissionControl.MissionControl), "AllowMissionControl")]
    class MissionControl_AllowMissionControl
    {
        public static void Postfix(MissionControl.MissionControl __instance, ref bool __result)
        {
            if (!__result && (__instance.CurrentContract.IsRestorationContract || __instance.CurrentContract.IsStoryContract))
            {
                bool ena = MissionControl.Main.Settings.ContractSettingsOverrides.ContainsKey(__instance.CurrentContract.Override.ID);
                __result = ena;
            }
        }
    }

    [HarmonyPatch(typeof(EncounterRules), "GetPlayerLanceSpawnerName")]
    class EncounterRules_GetPlayerLanceSpawnerName
    {
        public static void Postfix(ref string __result)
        {
            string type = MissionControl.MissionControl.Instance.CurrentContract.ContractTypeValue.Name;
            if (type == "Panzyr_Attack")
                __result = "PlayerLanceSpawner_Attack";
            else if (type == "Smithon_Attack")
                __result = "PlayerLanceSpawner";
        }
    }

    [HarmonyPatch(typeof(EncounterRules), "GetPlayerLanceChunkName")]
    class EncounterRules_GetPlayerLanceChunkName
    {
        public static void Postfix(ref string __result)
        {
            //foreach (Transform ch in MissionControl.MissionControl.Instance.EncounterLayerGameObject.transform)
            //{
            //    FileLog.Log(ch.name);
            //    foreach (Transform chch in ch.transform)
            //    {
            //        FileLog.Log("\t" + chch.name);
            //    }
            //}
            string type = MissionControl.MissionControl.Instance.CurrentContract.ContractTypeValue.Name;
            if (type == "Story_5_ServedCold")
                __result = "01_InitialSetup/Chunk_PlayerLance";
        }
    }
}
