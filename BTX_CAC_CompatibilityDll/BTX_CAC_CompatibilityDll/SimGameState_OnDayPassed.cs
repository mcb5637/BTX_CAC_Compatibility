using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    class SimGameState_OnDayPassed
    {
        public static void Postfix(SimGameState __instance)
        {
            if (HasBullShark(__instance) && !__instance.CompanyTags.Contains("bullshark_cac_lt_upgrade") && __instance.GetFirstFreeMechBay() >= 0 && __instance.Funds> 10000000 && __instance.NetworkRandom.Int(0, 100) < 10)
                ShowLongTomSpecial(__instance);
        }

        private static bool HasBullShark(SimGameState s)
        {
            if (s.GetItemCount("chassisdef_bullshark_BSK-MAZ", typeof(MechDef), SimGameState.ItemCountType.UNDAMAGED_ONLY) > 0)
                return true;
            return false;
        }

        private static void ShowLongTomSpecial(SimGameState s)
        {
            s.CompanyTags.Add("bullshark_cac_lt_upgrade");
            s.InterruptQueue.QueuePauseNotification("Yangs Offer",
                $"Hey, Boss. We have a Bull Shark and I found a Long Tom on the local market. Give me {SimGameState.GetCBillString(10000000)} and a lot of time, and I replace the Thumper with it.",
                s.GetCrewPortrait(SimGameCrew.Crew_Yang), "", () =>
                {
                    RemoveBullshark(s);
                    AddBullsharkLT(s);
                    s.AddFunds(-10000000, null, true, true);
                }, "OK", () =>
                {
                    s.CompanyTags.Remove("bullshark_cac_lt_upgrade");
                }, "Cancel");
        }

        private static void RemoveBullshark(SimGameState s)
        {
            s.RemoveItemStat("chassisdef_bullshark_BSK-MAZ", typeof(MechDef), false);
        }

        private static void AddBullsharkLT(SimGameState s)
        {
            MechDef d = s.DataManager.MechDefs.Get("mechdef_bullshark_BSK-LT");
            d = new MechDef(d, s.GenerateSimGameUID(), true);
            d.SetInventory(d.Inventory.Where((x) => x.IsFixed || x.ComponentDefID.Equals("Ammo_AmmunitionBox_Generic_LongTom")).ToArray());
            int baySlot = s.GetFirstFreeMechBay();
            int mechReadyTime = 625000; // about 50 days
            WorkOrderEntry_ReadyMech workOrderEntry_ReadyMech = new WorkOrderEntry_ReadyMech(string.Format("ReadyMech-{0}", d.GUID), string.Format("Readying 'Mech - {0}", new object[]
                {
                    d.Chassis.Description.Name
                }), mechReadyTime, baySlot, d, string.Format(s.Constants.Story.MechReadiedWorkOrderCompletedText, new object[]
                {
                    d.Chassis.Description.Name
                }));
            s.MechLabQueue.Add(workOrderEntry_ReadyMech);
            s.ReadyingMechs[baySlot] = d;
            s.RoomManager.AddWorkQueueEntry(workOrderEntry_ReadyMech);
            s.UpdateMechLabWorkQueue(false);
            AudioEventManager.PlayAudioEvent("audioeventdef_simgame_vo_barks", "workqueue_readymech", WwiseManager.GlobalAudioObject, null);
        }
    }
}
