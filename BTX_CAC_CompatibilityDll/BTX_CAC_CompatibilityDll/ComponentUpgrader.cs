using BattleTech;
using BTRandomMechComponentUpgrader;
using CustomUnits;
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

        private static void SmartAmmoAdjust(MechDef m, SimGameState s, UpgradeList l, float canFreeTonns, AmmoTracker ammo, MechDef fromData, FactionValue team)
        {
            bool pirate = team.IsPirate;
            bool kurita = team.Name.StartsWith("Kurita");
            bool davion = team.Name.StartsWith("Davion");
            var rand = s.NetworkRandom;
            var mood = s.SelectedContract?.mapMood;
            if (mood == null)
                Main.Log.Log("warning: contract mood null");
            Main.Log.Log($"handling {m.Description.Id} of {team.Name} in mood {mood.SafeToString()}");
            foreach (var kv in ammo.AmmoGroups)
            {
                if (kv.Key == "")
                    continue;
                Dictionary<string, int> ideal = kv.Value.IdealAmmoRatios;
                Main.Log.Log($"handling group {kv.Key}");
                if (kv.Key == "SRM")
                {
                    var ammos = kv.Value.LongestSublist.Get(SubListType.Ammo);
                    var std = ammos.FirstOrDefault(x => x.ID == "Ammo_AmmunitionBox_Generic_SRM");
                    var inf = ammos.FirstOrDefault(x => x.ID == "Ammo_AmmunitionBox_Generic_SRMInferno" && !kv.Value.AmmoLockout.Contains(x.ID));
                    var df = ammos.FirstOrDefault(x => x.ID == "Ammo_AmmunitionBox_Generic_SRM_DF" && !kv.Value.AmmoLockout.Contains(x.ID));
                    if (inf != null && ideal[inf.ID] < ideal[std.ID])
                        inf = null;
                    if (df != null && ideal[df.ID] < ideal[std.ID])
                        df = null;
                    ideal.Clear();
                    ideal[std.ID] = 1;
                    if (pirate && inf != null && rand.Float() < (AEPStatic.GetTimelineSettings().PirateInfernoChance / 100.0f))
                    {
                        ideal[inf.ID] = 10;
                        Main.Log.Log("pyromaniac");
                    }
                    if (kurita && df != null && df.MinDate <= s.CurrentDate)
                    {
                        ideal[df.ID] = 1;
                        Main.Log.Log("add DF");
                    }
                    kv.Value.CalculateIdealBoxes();
                }
                else if (kv.Key.StartsWith("AC"))
                {
                    var ammos = kv.Value.LongestSublist.Get(SubListType.Ammo);
                    var std = ammos.FirstOrDefault(); // default ammo always first
                    var tracer = ammos.FirstOrDefault(x => x.ID.EndsWith("Tracer") && !kv.Value.AmmoLockout.Contains(x.ID));
                    var prec = ammos.FirstOrDefault(x => x.ID.EndsWith("Precision") && !kv.Value.AmmoLockout.Contains(x.ID));
                    var ap = ammos.FirstOrDefault(x => x.ID.EndsWith("AP") && !kv.Value.AmmoLockout.Contains(x.ID));

                    if (!ideal.TryGetValue(std.ID, out var stdcount))
                        stdcount = 0;
                    if (tracer == null || !ideal.TryGetValue(tracer.ID, out var tracercount))
                        tracercount = 0;
                    if (stdcount > tracercount)
                    {
                        Main.Log.Log("has uacs, no special ammo");
                        if (tracer != null)
                            ideal[tracer.ID] = 0;
                        if (prec != null)
                            ideal[prec.ID] = 0;
                        if (ap != null)
                            ideal[ap.ID] = 0;
                        continue;
                    }

                    if (mood == null || !(mood.Contains("Night") || mood.Contains("Sunset") || mood.Contains("Twilight")))
                        tracer = null;
                    if (prec != null && prec.MinDate > s.CurrentDate)
                        prec = null;
                    if (ap != null && ap.MinDate > s.CurrentDate)
                        ap = null;
                    ideal.Clear();
                    ideal[std.ID] = 1;
                    if (tracer != null)
                    {
                        Main.Log.Log("add tracer");
                        ideal[tracer.ID] = 2;
                    }
                    if (davion && prec != null)
                    {
                        Main.Log.Log("add precision");
                        ideal[prec.ID] = 1;
                    }
                    if (davion && ap != null)
                    {
                        Main.Log.Log("add ap");
                        ideal[ap.ID] = 1;
                    }
                    kv.Value.CalculateIdealBoxes();
                }
                else if (kv.Key == "LRM")
                {
                    var ammos = kv.Value.LongestSublist.Get(SubListType.Ammo);
                    var std = ammos.FirstOrDefault(x => x.ID == "Ammo_AmmunitionBox_Generic_LRM");
                    var df = ammos.FirstOrDefault(x => x.ID == "Ammo_AmmunitionBox_Generic_LRM_DF" && !kv.Value.AmmoLockout.Contains(x.ID));
                    if (df != null && ideal[df.ID] < ideal[std.ID])
                        df = null;
                    ideal.Clear();
                    ideal[std.ID] = 1;
                    if (kurita && df != null && df.MinDate <= s.CurrentDate)
                    {
                        ideal[df.ID] = 1;
                        Main.Log.Log("add DF");
                    }
                    kv.Value.CalculateIdealBoxes();
                }
            }
        }

        public static void Register()
        {
            MechProcessor.GetUpgradeList = GetUpgradeList;
            Modifier_AmmoSwapper.SmartAmmoAdjust = SmartAmmoAdjust;
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
