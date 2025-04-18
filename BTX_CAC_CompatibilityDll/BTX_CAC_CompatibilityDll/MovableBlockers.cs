﻿using BattleTech;
using BattleTech.UI;
using CustomComponents;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using BattleTech.Data;
using CustomComponents.Changes;
using UIWidgets;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch]
    public static class MovableBlockers
    {
        [CustomComponent("BlockerList")]
        public class CustomBlockerList : SimpleCustomChassis
        {
            public string Category;
            public DefaultsInfoRecord[] Blockers;
        }


        internal class BlockerCategory
        {
            internal string CategoryName;
            internal Regex Pattern;
        }
        private static readonly BlockerCategory[] BlockerCategories = new BlockerCategory[] {
            new BlockerCategory() {
                CategoryName = "EndoStructureBlocker",
                Pattern = new Regex("Gear_EndoSteel_(?<i>\\d+)_Slot"),
            },
            new BlockerCategory() {
                CategoryName = "FerroStructureBlocker",
                Pattern = new Regex("Gear_FerroFibrous_(?<i>\\d+)_Slot"),
            },
            new BlockerCategory() {
                CategoryName = "ComboStructureBlocker",
                Pattern = new Regex("Gear_EndoFerroCombo_(?<i>\\d+)_Slot"),
            },
        }; // TODO config
        private static IEnumerable<string> Categories => BlockerCategories.Select(x => x.CategoryName);

        private static bool HasLimits(this ChassisDef d)
        {
            return d.GetComponents<ChassisCategory>().Any((x) => Categories.Contains(x.CategoryID));
        }

        private static int GetLimit(this ChassisDef d, string cat)
        {
            return d.GetComponents<ChassisCategory>().FirstOrDefault(x => x.CategoryID == cat)?.LocationLimits[ChassisLocations.All].Limit ?? 0;
        }

        private static int GetBlockerSize(this MechComponentRef d, string cat)
        {
            return d.GetCategory(cat)?.Weight ?? 0;
        }
        private static int GetBlockerSizeSave(this MechComponentRef d, BlockerCategory cat)
        {
            Match m = cat.Pattern.Match(d.ComponentDefID);
            if (!m.Success)
                return 0;
            if (int.TryParse(m.Groups["i"].Value, out int s))
                return s;
            return 0;
        }
        private static int GetBlockerSize(this MechComponentDef d, string cat)
        {
            return d.GetCategory(cat)?.Weight ?? 0;
        }
        private static bool IsBlocker(this MechComponentDef d, string cat)
        {
            return d.IsCategory(cat);
        }
        private static bool IsBlocker(this MechComponentRef d, string cat)
        {
            return d.IsCategory(cat);
        }
        private static bool IsBlockerSave(this MechComponentRef d, BlockerCategory cat)
        {
            return cat.Pattern.IsMatch(d.ComponentDefID);
        }
        public static bool IsBlocker(this MechComponentRef d)
        {
            return Categories.Any((cat) => d.IsCategory(cat));
        }
        public static bool IsBlockerSave(this MechComponentRef d)
        {
            return BlockerCategories.Any((cat) => d.IsBlockerSave(cat));
        }
        private static string GetBlockerCategory(this MechComponentRef d)
        {
            return Categories.FirstOrDefault((cat) => d.IsCategory(cat));
        }
        private static string GetBlockerCategorySave(this MechComponentRef d)
        {
            return BlockerCategories.FirstOrDefault((cat) => d.IsBlockerSave(cat))?.CategoryName;
        }

        private class BlockerDatabase
        {
            public Dictionary<string, MechComponentDef[]> Blockers = new Dictionary<string, MechComponentDef[]>();
        }

        private static BlockerDatabase Database_i = null;
        private static BlockerDatabase GetDatabase(DataManager m)
        {
            if (Database_i == null) // TODO config
            {
                Database_i = new BlockerDatabase();
                foreach (string c in Categories)
                {
                    MechComponentDef[] r = new MechComponentDef[8];
                    Database_i.Blockers[c] = r;
                }
                for (int i = 1; i <= 8; i++)
                {
                    MechComponentDef[] en = Database_i.Blockers["EndoStructureBlocker"];
                    MechComponentDef[] fe = Database_i.Blockers["FerroStructureBlocker"];
                    MechComponentDef[] co = Database_i.Blockers["ComboStructureBlocker"];
                    if (m.UpgradeDefs.TryGet($"Gear_EndoSteel_{i}_Slot", out UpgradeDef u)) {
                        int e = GetBlockerSize(u, "EndoStructureBlocker");
                        if (e > 0 && en[e - 1] == null)
                            en[e - 1] = u;
                    }
                    if (m.UpgradeDefs.TryGet($"Gear_FerroFibrous_{i}_Slot", out UpgradeDef f))
                    {
                        int e = GetBlockerSize(f, "FerroStructureBlocker");
                        if (e > 0 && fe[e - 1] == null)
                            fe[e - 1] = f;
                    }
                    if (m.UpgradeDefs.TryGet($"Gear_EndoFerroCombo_{i}_Slot", out UpgradeDef c))
                    {
                        int e = GetBlockerSize(c, "ComboStructureBlocker");
                        if (e > 0 && co[e - 1] == null)
                            co[e - 1] = c;
                    }
                }
                //foreach (var kv in Database_i.Blockers)
                //{
                //    FileLog.Log($"blocker {kv.Key}");
                //    for (int i = 0; i < kv.Value.Length; i++)
                //    {
                //        FileLog.Log($"{kv.Value[i]?.Description.Id ?? "null"} {i + 1} {GetBlockerSize(kv.Value[i], kv.Key)}");
                //    }
                //}
            }
            return Database_i;
        }

        private static MechComponentDef GetBlocker(int s, DataManager m, string cat)
        {
            BlockerDatabase d = GetDatabase(m);
            s--;
            if (s < 0 || s >= d.Blockers[cat].Length)
                return null;
            return d.Blockers[cat][s];
        }

        public static void FixChassisDef(ChassisDef d, List<MechComponentRef> fixedinv)
        {
            if (fixedinv == null)
                return;
            if (HasLimits(d))
            {
                fixedinv.RemoveAll(IsBlockerSave);
                return;
            }
            if (fixedinv.Count == 0)
                return;

            List<DefaultsInfoRecord> def = new List<DefaultsInfoRecord>();
            foreach (BlockerCategory cat in BlockerCategories)
            {
                int l = fixedinv.Select((x) => x.GetBlockerSizeSave(cat)).Sum();
                if (l > 0)
                {
                    SetCategoryLimit(cat.CategoryName, l, d);
                    DefaultsInfoRecord[] b = fixedinv.Where((x) => x.IsBlockerSave(cat)).Select((x) => new DefaultsInfoRecord()
                        {
                            Location = x.MountedLocation,
                            DefID = x.ComponentDefID,
                            Type = x.ComponentDefType,
                        }).ToArray();
                    SetCategoryDefaults(cat.CategoryName, b, d);
                }
            }

            fixedinv.RemoveAll(IsBlockerSave);
        }

        public static void FixMechInventory(ChassisDef d, List<MechComponentRef> inv)
        {
            foreach (string cat in Categories)
            {
                int l = d.GetLimit(cat);
                int bl = inv.Select((x) => x.GetBlockerSize(cat)).Sum();
                if (l > 0 && l != bl && bl > 0)
                {
                    Main.Log.LogWarning($"{d.Description.Id} nuke blockers {cat} (was {bl} expeced {l})");
                    inv.RemoveAll((x) => x.IsBlocker(cat));
                }
            }
            foreach (CustomBlockerList c in d.GetComponents<CustomBlockerList>())
            {
                if (inv.Any((x) => x.IsBlocker(c.Category)))
                {
                    //FileLog.Log($"{d.Description.Id} skip blockers {c.Category}");
                    continue;
                }
                //FileLog.Log($"{d.Description.Id} add blockers {c.Category}");
                foreach (DefaultsInfoRecord b in c.Blockers)
                {
                    AddToInvRaw(b.DefID, b.Location, b.Type, d.DataManager, inv);
                }
            }
            foreach (MechComponentRef c in inv)
            {
                if (c.IsBlocker() && c.SimGameUID != null && c.SimGameUID.StartsWith("FixedEquipment-"))
                    c.SetSimGameUID(c.SimGameUID.Replace("FixedEquipment-", ""));
            }
        }

        private static void AddToInvRaw(string id, ChassisLocations c, ComponentType t, DataManager m, List<MechComponentRef> inv)
        {
            MechComponentRef a = new MechComponentRef(id, null, t, c, -1, ComponentDamageLevel.Functional, false)
            {
                DataManager = m
            };
            a.RefreshComponentDef();
            inv.Add(a);
        }

        private static void SetCategoryLimit(string cat, int l, ChassisDef d)
        {
            ChassisCategory c = d.GetComponents<ChassisCategory>().FirstOrDefault((x) => x.CategoryID == cat);
            if (c == null)
            {
                c = new ChassisCategory
                {
                    CategoryID = cat,
                };
                d.AddComponent(c);
            }
            c.LocationLimits = new Dictionary<ChassisLocations, CategoryLimit>
            {
                [ChassisLocations.All] = new CategoryLimit(l, l, false)
            };
        }

        private static void SetCategoryDefaults(string cat, DefaultsInfoRecord[] b, ChassisDef d)
        {
            CustomBlockerList c = d.GetComponents<CustomBlockerList>().FirstOrDefault((x) => x.Category == cat);
            if (c == null)
            {
                c = new CustomBlockerList
                {
                    Category = cat,
                };
                d.AddComponent(c);
            }
            c.Blockers = b;
        }

        public static void RegisterValidators()
        {
            Validator.RegisterDropValidator(null, ReplaceValidateDropDelegate, null);
        }

        private static readonly ChassisLocations[] LocSearch = new ChassisLocations[] {
            ChassisLocations.LeftArm,
            ChassisLocations.RightArm,
            ChassisLocations.LeftLeg,
            ChassisLocations.RightLeg,
            ChassisLocations.LeftTorso,
            ChassisLocations.RightTorso,
            ChassisLocations.CenterTorso,
            ChassisLocations.Head,
        };

        //public static void DumpInv(this MechDef m)
        //{
        //    FileLog.Log($"dumping {m.Description.Id}");
        //    foreach (var c in m.Inventory)
        //    {
        //        FileLog.Log($" {c.ComponentDefID} {c.MountedLocation} {c.IsFixed} {GetBlockerCategory(c) ?? "null"}");
        //    }
        //    FileLog.Log($"dumping {m.Chassis.Description.Id}");
        //    foreach (var c in m.Chassis.FixedEquipment)
        //    {
        //        FileLog.Log($" {c.ComponentDefID} {c.MountedLocation} {c.IsFixed} {GetBlockerCategory(c) ?? "null"}");
        //    }
        //    foreach (CustomBlockerList c in m.Chassis.GetComponents<CustomBlockerList>())
        //    {
        //        FileLog.Log($"dumping {m.Chassis.Description.Id}.CustomBlockerList.{c.Category}");
        //        foreach (var r in c.Blockers)
        //        {
        //            FileLog.Log($" {r.DefID} {r.Location}");
        //        }
        //    }
        //}

        private static string ReplaceValidateDropDelegate(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {
            MechDef mechDef = MechLabHelper.CurrentMechLab.ActiveMech;
            ChassisDef ch = mechDef.Chassis;

            if (!HasLimits(mechDef.Chassis))
                return string.Empty;
            
            int slotsMax = ch.GetLocationDef(location).InventorySlots;
            int slotsNeeded = SlotsInLocation(location, drop_item, changes, mechDef) - slotsMax;

            if (slotsNeeded <= 0)
                return string.Empty;

            foreach (MechComponentRef c in mechDef.Inventory.Where((c) => c.MountedLocation == location && c.IsBlocker()))
            {
                if (slotsNeeded <= 0)
                    break;

                int slots = c.Def.InventorySize;
                string cat = c.GetBlockerCategory();
                checkLocs(c, cat, ref slots, true);
                if (slots > 0 && slotsNeeded > 0)
                    checkLocs(c, cat, ref slots, false);

                if (slots != c.Def.InventorySize)
                {
                    changes.Enqueue(new Change_Remove(c, c.MountedLocation));
                    if (slots > 0)
                        changes.Enqueue(CreateAdd(slots, c.DataManager, cat, c.MountedLocation));
                }
            }

            OptimizeBlockers(changes, mechDef);

            return string.Empty;

            void checkLocs(MechComponentRef c, string cat, ref int slots, bool sameloc)
            {
                foreach (ChassisLocations sea in LocSearch)
                {
                    if (sea == location)
                        continue;

                    bool issameloc = mechDef.Inventory.Any((x) => x.MountedLocation == sea && x.IsBlocker(cat));
                    if (sameloc && !issameloc)
                        continue;
                    if (!sameloc && issameloc)
                        continue;

                    int slotsAvail = ch.GetLocationDef(sea).InventorySlots - SlotsInLocation(sea, drop_item, changes, mechDef);
                    if (slotsAvail > 0)
                    {
                        int toMove = Math.Min(slotsAvail, slotsNeeded);
                        changes.Enqueue(CreateAdd(toMove, c.DataManager, cat, sea));
                        slotsNeeded -= toMove;
                        slots -= toMove;
                        if (slots <= 0)
                            break;
                        if (slotsNeeded <= 0)
                            break;
                    }
                }
            }
        }

        private static Change_Add CreateAdd(string id, ComponentType t, ChassisLocations l)
        {
            return new Change_Add(DefaultHelper.CreateSlot(id, t), l);
        }
        private static Change_Add CreateAdd(int s, DataManager dm, string cat, ChassisLocations l)
        {
            return CreateAdd(GetBlockerID(s, dm, cat), ComponentType.Upgrade, l);
        }

        private static int SlotsInLocation(ChassisLocations l, MechLabItemSlotElement drop_item, Queue<IChange> changes, MechDef mechDef)
        {
            int s = 0;
            foreach (MechComponentRef r in mechDef.Inventory)
            {
                if (r.MountedLocation == l)
                    s += r.Def.InventorySize;
            }
            if (drop_item.MountedLocation == l)
                s -= drop_item.ComponentRef.Def.InventorySize;
            foreach (IChange c in changes)
            {
                if (c is Change_Add a && a.Location == l)
                    s += GetComponentSize(a.ItemID, a.Type);
                else if (c is Change_Remove r && r.Location == l)
                    s -= GetComponentSize(r.ItemID, mechDef.DataManager);
            }
            return s;
        }

        private static string GetBlockerID(int s, DataManager m, string cat)
        {
            return GetBlocker(s, m, cat)?.Description.Id;
        }

        private static MechComponentDef GetComponent(string id, DataManager m)
        {
            if (m.UpgradeDefs.TryGet(id, out UpgradeDef u))
                return u;
            if (m.WeaponDefs.TryGet(id, out WeaponDef w))
                return w;
            if (m.JumpJetDefs.TryGet(id, out JumpJetDef j))
                return j;
            if (m.AmmoBoxDefs.TryGet(id, out AmmunitionBoxDef a))
                return a;
            if (m.HeatSinkDefs.TryGet(id, out HeatSinkDef h))
                return h;
            return null;
        }

        private static int GetComponentSize(string id, DataManager m)
        {
            return GetComponent(id, m)?.InventorySize ?? 0;
        }

        private static int GetComponentSize(string id, ComponentType t)
        {
            return DefaultHelper.GetComponentDef(id, t)?.InventorySize ?? 0;
        }

        private static void OptimizeBlockers(Queue<IChange> changes, MechDef m)
        {
            foreach (string cat in Categories)
            {
                foreach (ChassisLocations loc in LocSearch)
                {
                    int total = 0;
                    List<(int, MechComponentRef, Change_Add, string)> items = new List<(int, MechComponentRef, Change_Add, string)>();
                    foreach (MechComponentRef r in m.Inventory.Where((x) => x.MountedLocation == loc))
                    {
                        addInv(r, cat, ref total, items);
                    }
                    foreach (IChange c in changes)
                    {
                        if (c is Change_Add a && a.Location == loc)
                        {
                            addCh(a, cat, ref total, items);
                        }
                        else if (c is Change_Remove r && r.Location == loc)
                        {
                            remCh(r, cat, ref total, items, m.DataManager);
                        }
                    }

                    opt(total, items, cat, loc);
                }
            }

            void addInv(MechComponentRef r, string cat, ref int total, List<(int, MechComponentRef, Change_Add, string)> l)
            {
                if (r.IsBlocker(cat))
                {
                    int s = GetBlockerSize(r.Def, cat);
                    total += s;
                    l.Add((s, r, null, r.ComponentDefID));
                }
            }
            void addCh(Change_Add a, string bl, ref int total, List<(int, MechComponentRef, Change_Add, string)> l)
            {
                MechComponentDef comp = DefaultHelper.GetComponentDef(a.ItemID, a.Type);
                if (comp.IsBlocker(bl))
                {
                    int s = GetBlockerSize(comp, bl);
                    total += s;
                    l.Add((s, null, a, a.ItemID));
                }
            }
            void remCh(Change_Remove r, string cat, ref int total, List<(int, MechComponentRef, Change_Add, string)> l, DataManager dm)
            {
                MechComponentDef comp = GetComponent(r.ItemID, dm);
                if (comp.IsBlocker(cat))
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        if (l[i].Item4 == r.ItemID)
                        {
                            total -= l[i].Item1;
                            l.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            void remAll(List<(int, MechComponentRef, Change_Add, string)> l)
            {
                foreach ((int s, MechComponentRef r, Change_Add c, string _) in l)
                {
                    if (r != null)
                        changes.Enqueue(new Change_Remove(r.ComponentDefID, r.MountedLocation));
                    else if (c != null)
                        changes.Enqueue(new Change_Remove(c.ItemID, c.Location));
                }
            }
            void opt(int total, List<(int, MechComponentRef, Change_Add, string)> l, string cat, ChassisLocations loc)
            {
                if (l.Count > 1 && total <= 8)
                {
                    remAll(l);
                    changes.Enqueue(CreateAdd(total, m.DataManager, cat, loc));
                }
                else if (l.Count > 2)
                {
                    remAll(l);
                    changes.Enqueue(CreateAdd(8, m.DataManager, cat, loc));
                    changes.Enqueue(CreateAdd(total - 8, m.DataManager, cat, loc));
                }
            }
        }

        [HarmonyPatch(typeof(ChassisDef), nameof(ChassisDef.FromJSON))]
        [HarmonyPostfix]
        public static void ChassisDef_FromJSON(ChassisDef __instance)
        {
            try
            {
                if (__instance.FixedEquipment == null || __instance.FixedEquipment.Length == 0)
                    return;
                List<MechComponentRef> inv = __instance.FixedEquipment.ToList();
                FixChassisDef(__instance, inv);
                __instance.SetFixedEquipment(inv.ToArray());
            }
            catch (Exception e)
            {
                FileLog.Log($"except chassis {__instance.Description.Id} {e}");
            }
        }
    }
}
