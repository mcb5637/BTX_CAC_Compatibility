using BattleTech;
using BTRandomMechComponentUpgrader;
using FullXotlTables;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    internal static class ComponentUpgrader
    {
        public static UpgradeList GetUpgradeList(FactionValue f)
        {
            SimGameState s = UnityGameInstance.BattleTechGame.Simulation;
            if (!s.CompanyTags.Contains("CAC_C_UpgradedComponents"))
                return null;

            string fs = f.ToString();
            if (f.IsPirate)
                fs = "DavionLocals"; // TODO
            else if (f.IsClan)
                fs = "ClansB";
            else if (fs == "WolfsDragoons" || fs == "BlackWidowCompany")
                fs = "DavionA";
            else if (fs == "Merc28") // snords
                fs = "DavionA";
            else if (fs == "AuriganDirectorate" || fs == "AuriganRestoration" || fs == "Betrayers" || fs == "ChaosMarch"
                || fs == "Locals" || fs == "Arc-RoyalDC") // some random peripheries
                fs = "DavionD";
            else if (fs == "ComStar" || fs == "WordOfBlake" || fs == "Moderbjorn" || fs == "Nautilus")
            {
                if (s.CompanyTags.Contains("CAC_C_UpgradedComponentsCSP"))
                    fs = "ComStarPlus";
            }
            UpgradeList l = LookupUpgradeList(fs);
            if (l != null)
                return l;
            if (AEPStatic.GetXotlSettings().UnitTableReferences.TryGetValue(fs, out UnitInfo list))
            {
                l = LookupUpgradeList(list.PrimaryFaction);
                if (l != null)
                    return l;
            }
            return LookupUpgradeList("DavionLocals"); // fallback, mostly some random criminals that fall down here
        }
        private static UpgradeList LookupUpgradeList(string s)
        {
            UpgradeList l = MechProcessor.UpgradeLists.FirstOrDefault((x) => x.Factions.Contains(s));
            if (l == null)
            {
                s += "C";
                l = MechProcessor.UpgradeLists.FirstOrDefault((x) => x.Factions.Contains(s));
            }
            return l;
        }

        public static void Register()
        {
            MechProcessor.GetUpgradeList = GetUpgradeList;
        }

        //[HarmonyPatch(typeof(SimGameState), "SetSimRoomState")]
        //internal class SimGameState_SetSimRoomState
        //{
        //    public static void Prefix(SimGameState __instance, DropshipLocation state)
        //    {
        //        if (state == DropshipLocation.CMD_CENTER && Input.GetKey(KeyCode.LeftShift))
        //        {
        //            try
        //            {
        //                foreach (KeyValuePair<string, FactionDef> f in __instance.DataManager.Factions)
        //                {
        //                    var l = GetUpgradeList(f.Value.FactionValue);
        //                    FileLog.Log($"upgradelist for str {f.Key}/{f.Value.FactionValue} -> {(l == null ? "null" : l.Name)}");
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                FileLog.Log(e.ToString());
        //            }
        //        }
        //    }
        //}
    }
}
