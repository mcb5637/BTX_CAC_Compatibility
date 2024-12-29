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
        private static readonly string[] Categories = new string[] { "EndoStructureBlocker", "FerroStructureBlocker" }; // TODO config

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
        private static bool IsBlocker(this MechComponentRef d)
        {
            return Categories.Any((cat) => d.IsCategory(cat));
        }
        private static string GetBlockerCategory(this MechComponentRef d)
        {
            return Categories.FirstOrDefault((cat) => d.IsCategory(cat));
        }
        private static bool IsComboBlocker(this MechComponentRef d) // TODO config
        {
            return d.ComponentDefID.StartsWith("Gear_EndoFerroCombo_");
        }
        private static int GetComboBlockerSize(string id) // TODO config
        {
            return int.Parse(id.Replace("Gear_EndoFerroCombo_", "").Replace("_Slot", ""));
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
                }
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
        private static MechComponentDef GetEndoBlocker(int s, DataManager m)
        {
            return GetBlocker(s, m, "EndoStructureBlocker");
        }
        private static MechComponentDef GetFerroBlocker(int s, DataManager m)
        {
            return GetBlocker(s, m, "FerroStructureBlocker");
        }

        public static bool HandleChassisDef(ChassisDef d, List<MechComponentRef> fixedinv, List<MechComponentRef> inv)
        {
            if (fixedinv == null || fixedinv.Count == 0)
                return false;
            if (HasLimits(d))
                return false;
            int endoSlots = 0;
            int ferroSlots = 0;
            int comboSlots = 0;
            foreach (MechComponentRef c in fixedinv)
            {
                if (c.IsBlocker("EndoStructureBlocker"))
                    endoSlots += GetBlockerSize(c.Def, "EndoStructureBlocker");
                else if (c.IsBlocker("FerroStructureBlocker"))
                    ferroSlots += GetBlockerSize(c.Def, "FerroStructureBlocker");
                else if (c.IsComboBlocker())
                    comboSlots += GetComboBlockerSize(c.ComponentDefID);
            }
            if (comboSlots > 0)
            {
                int f = comboSlots / 2;
                int e = comboSlots - f;
                ferroSlots += f;
                endoSlots += e;
                while (e + f > 0)
                {
                    MechComponentRef c = fixedinv.FirstOrDefault((x) => x.IsComboBlocker());
                    if (c == null)
                        break;
                    int cs = GetComboBlockerSize(c.ComponentDefID);
                    if (cs <= e)
                    {
                        e -= cs;
                        c.ComponentDefID = GetEndoBlocker(cs, c.DataManager).Description.Id;
                        c.RefreshComponentDef();
                    }
                    else if (e > 0)
                    {
                        int newf = cs - e;
                        c.ComponentDefID = GetEndoBlocker(e, c.DataManager).Description.Id;
                        c.RefreshComponentDef();
                        e = 0;
                        AddToInvRaw(GetFerroBlocker(newf, c.DataManager).Description.Id, c.MountedLocation, c.DataManager, fixedinv);
                        f -= newf;
                    }
                    else
                    {
                        f -= cs;
                        c.ComponentDefID = GetFerroBlocker(cs, c.DataManager).Description.Id;
                        c.RefreshComponentDef();
                    }
                }
            }

            inv.RemoveAll((c) => c.IsBlocker() || c.IsComboBlocker());
            inv.InsertRange(0, fixedinv.Where((c) => c.IsBlocker()));
            fixedinv.RemoveAll((c) => c.IsBlocker() || c.IsComboBlocker());
            foreach (MechComponentRef c in inv.Where((c) => c.IsBlocker()))
            {
                c.SetIsFixed(false);
            }

            if (endoSlots > 0 || ferroSlots > 0)
            {
                SetCategoryLimit("EndoStructureBlocker", endoSlots, d);
                SetCategoryLimit("FerroStructureBlocker", ferroSlots, d);
            }
            return endoSlots > 0 || ferroSlots > 0;
        }

        private static void AddToInvRaw(string id, ChassisLocations c, DataManager m, List<MechComponentRef> inv)
        {
            MechComponentRef a = new MechComponentRef(id, null, ComponentType.Upgrade, c, -1, ComponentDamageLevel.Functional, true)
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
