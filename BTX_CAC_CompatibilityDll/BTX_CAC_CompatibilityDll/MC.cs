using Harmony;
using MissionControl.Rules;
using System.Reflection;

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
}
