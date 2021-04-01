using BattleTech;
using BattleTech.Designed;
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
            else if (type=="Smithon_Attack" || type=="Story_6A_TreasureTrove")
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
            //        foreach (Transform chchc in chch.transform)
            //        {
            //            FileLog.Log("\t\t" + chchc.name);
            //        }
            //    }
            //}
            string type = MissionControl.MissionControl.Instance.CurrentContract.ContractTypeValue.Name;
            if (type == "Story_5_ServedCold")
                __result = "01_InitialSetup/Chunk_PlayerLance";
            else if (type == "Story_6A_TreasureTrove")
                __result = "Gen_PlayerLance";
        }
    }

    [HarmonyPatch(typeof(SceneUtils), "GetRandomPositionWithinBounds")]
    class SceneUtils_GetRandomPositionWithinBounds
    {
        public static bool Prefix(Vector3 target, float maxDistance, ref Vector3 __result)
        {

            GameObject chunkBoundaryRect = MissionControl.MissionControl.Instance.EncounterLayerGameObject.transform.Find("Chunk_EncounterBoundary")?.gameObject;
            if (chunkBoundaryRect == null)
                chunkBoundaryRect = MissionControl.MissionControl.Instance.EncounterLayerGameObject.transform.Find("Gen_EncounterBoundary").gameObject;
            GameObject boundary = chunkBoundaryRect.transform.Find("EncounterBoundaryRect").gameObject;
            EncounterBoundaryChunkGameLogic chunkBoundary = chunkBoundaryRect.GetComponent<EncounterBoundaryChunkGameLogic>();
            EncounterBoundaryRectGameLogic boundaryLogic = boundary.GetComponent<EncounterBoundaryRectGameLogic>();
            Rect boundaryRec = chunkBoundary.GetEncounterBoundaryRectBounds();

            Vector3 randomRecPosition = boundaryRec.GetRandomPositionFromTarget(target, maxDistance);
            __result = randomRecPosition.GetClosestHexLerpedPointOnGrid();
            return false;
        }
    }
}
