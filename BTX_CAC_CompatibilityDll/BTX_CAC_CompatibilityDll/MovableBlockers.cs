using BattleTech;
using BattleTech.UI;
using CustomComponents;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using BattleTech.Data;
using CustomComponents.Changes;
using UIWidgets;

namespace BTX_CAC_CompatibilityDll
{
    public static class MovableBlockers
    {
        [CustomComponent("BlockerList")]
        public class CustomBlockerList : SimpleCustomComponent
        {
            public string Category;
            public DefaultsInfoRecord[] Blockers;
        }

        private static readonly string[] Categories = new string[] { "EndoStructureBlocker", "FerroStructureBlocker", "ComboStructureBlocker" }; // TODO config

        private static bool HasLimits(this ChassisDef d)
        {
            return d.GetComponents<ChassisCategory>().Any((x) => Categories.Contains(x.CategoryID));
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
        public static bool IsBlocker(this MechComponentRef d)
        {
            return Categories.Any((cat) => d.IsCategory(cat));
        }
        private static string GetBlockerCategory(this MechComponentRef d)
        {
            return Categories.FirstOrDefault((cat) => d.IsCategory(cat));
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

        public static void HandleMech(MechDef m, List<MechComponentRef> fixedinv, List<MechComponentRef> inv)
        {
            HandleChassisDef(m, fixedinv, inv);
            HandleMechDef(m, inv);
        }

        private static void HandleChassisDef(MechDef m, List<MechComponentRef> fixedinv, List<MechComponentRef> inv)
        {
            ChassisDef d = m.Chassis;
            if (fixedinv == null || fixedinv.Count == 0)
                return;
            if (HasLimits(d))
                return;
            int endoSlots = 0;
            int ferroSlots = 0;
            int comboSlots = 0;
            List<DefaultsInfoRecord> endos = new List<DefaultsInfoRecord>();
            List<DefaultsInfoRecord> ferros = new List<DefaultsInfoRecord>();
            List<DefaultsInfoRecord> combos = new List<DefaultsInfoRecord>();
            foreach (MechComponentRef c in fixedinv)
            {
                if (c.IsBlocker("EndoStructureBlocker"))
                {
                    endoSlots += GetBlockerSize(c.Def, "EndoStructureBlocker");
                    endos.Add(new DefaultsInfoRecord()
                    {
                        Location = c.MountedLocation,
                        DefID = c.ComponentDefID,
                        Type = c.ComponentDefType,
                    });
                }
                else if (c.IsBlocker("FerroStructureBlocker"))
                {
                    ferroSlots += GetBlockerSize(c.Def, "FerroStructureBlocker");
                    ferros.Add(new DefaultsInfoRecord()
                    {
                        Location = c.MountedLocation,
                        DefID = c.ComponentDefID,
                        Type = c.ComponentDefType,
                    });
                }
                else if (c.IsBlocker("ComboStructureBlocker"))
                {
                    comboSlots += GetBlockerSize(c.Def, "ComboStructureBlocker");
                    combos.Add(new DefaultsInfoRecord()
                    {
                        Location = c.MountedLocation,
                        DefID = c.ComponentDefID,
                        Type = c.ComponentDefType,
                    });
                }
            }
            
            inv.RemoveAll(IsBlocker);
            fixedinv.RemoveAll(IsBlocker);

            if (endoSlots > 0)
            {
                SetCategoryLimit("EndoStructureBlocker", endoSlots, d);
                SetCategoryDefaults("EndoStructureBlocker", endos, d);
                return;
            }
            if (ferroSlots > 0)
            {
                SetCategoryLimit("FerroStructureBlocker", ferroSlots, d);
                SetCategoryDefaults("FerroStructureBlocker", ferros, d);
                return;
            }
            if (comboSlots > 0)
            {
                SetCategoryLimit("ComboStructureBlocker", comboSlots, d);
                SetCategoryDefaults("ComboStructureBlocker", combos, d);
                return;
            }
        }

        private static void HandleMechDef(MechDef m, List<MechComponentRef> inv)
        {
            ChassisDef d = m.Chassis;
            foreach (CustomBlockerList c in d.GetComponents<CustomBlockerList>())
            {
                if (inv.Any((x) => x.IsBlocker(c.Category)))
                {
                    continue;
                }
                foreach (DefaultsInfoRecord b in c.Blockers)
                {
                    AddToInvRaw(b.DefID, b.Location, b.Type, m.DataManager, inv);
                }
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

        private static void SetCategoryDefaults(string cat, List<DefaultsInfoRecord> l, ChassisDef d)
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
            c.Blockers = l.ToArray();
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
        //        if (c.IsBlocker())
        //            c.SetIsFixed( false );
        //    }
        //    FileLog.Log($"dumping {m.Chassis.Description.Id}");
        //    foreach (var c in m.Chassis.FixedEquipment)
        //    {
        //        FileLog.Log($" {c.ComponentDefID} {c.MountedLocation} {c.IsFixed} {GetBlockerCategory(c) ?? "null"}");
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
                        changes.Enqueue(new Change_Add(GetBlockerID(slots, c.DataManager, cat), ComponentType.Upgrade, c.MountedLocation));
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
                        changes.Enqueue(new Change_Add(GetBlockerID(toMove, c.DataManager, cat), ComponentType.Upgrade, sea));
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
                    changes.Enqueue(new Change_Add(GetBlockerID(total, m.DataManager, cat), ComponentType.Upgrade, loc));
                }
                else if (l.Count > 2)
                {
                    remAll(l);
                    changes.Enqueue(new Change_Add(GetBlockerID(8, m.DataManager, cat), ComponentType.Upgrade, loc));
                    changes.Enqueue(new Change_Add(GetBlockerID(total - 8, m.DataManager, cat), ComponentType.Upgrade, loc));
                }
            }
        }
    }
}
