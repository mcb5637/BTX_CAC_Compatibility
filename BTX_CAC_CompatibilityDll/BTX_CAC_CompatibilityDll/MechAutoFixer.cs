using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomComponents;
using BattleTech.Save.SaveGameStructure;
using HarmonyLib;

namespace BTX_CAC_CompatibilityDll
{
    internal class MechAutoFixer
    {
        internal static void Fixer(List<MechDef> mechs)
        {
            foreach (MechDef mech in mechs)
            {
                try
                {
                    SplitAddons(mech);
                }
                catch (Exception e)
                {
                    FileLog.Log($"except mech {mech.Description.Id} {e}");
                }
            }
        }

        private static readonly ChassisLocations[] AllLocs = new ChassisLocations[]
        {
            ChassisLocations.LeftTorso,
            ChassisLocations.RightTorso,
            ChassisLocations.CenterTorso,
            ChassisLocations.LeftArm,
            ChassisLocations.RightArm,
            ChassisLocations.LeftLeg,
            ChassisLocations.RightLeg,
            ChassisLocations.Head,
        };
        private static void SplitAddons(MechDef m)
        {
            bool hasChange = false;
            List<MechComponentRef> mechinv = m.Inventory.ToList();
            mechinv.RemoveAll((x) => x.IsFixed); // gets re-added by setinv
            List<MechComponentRef> fixedinv = m.Chassis.FixedEquipment?.ToList();
            check(mechinv, false);
            if (fixedinv != null)
                check(fixedinv, true);
            if (hasChange)
            {
                if (fixedinv != null)
                    m.Chassis.SetFixedEquipment(fixedinv.ToArray());
                m.SetInventory(mechinv.ToArray());
                m.Chassis.RefreshLocationReferences();
            }

            int slotsleft(ChassisLocations loc)
            {
                int usage = mechinv.Where((x) => x.MountedLocation == loc).Select((x) => {
                    if (x.Def == null)
                        x.RefreshComponentDef();
                    if (x.Def == null)
                    {
                        FileLog.Log($"found null comp {x.ComponentDefID} {x.ComponentDefType} in {m.Description.Id}");
                        return 0;
                    }
                    return x.Def.InventorySize;
                }).Sum();
                int max = m.Chassis.GetLocationDef(loc).InventorySlots;
                return max - usage;
            }
            void check(List<MechComponentRef> l, bool f)
            {
                for (int i = 0; i < l.Count; i++)
                {
                    if (Main.Splits.TryGetValue(l[i].ComponentDefID, out WeaponAddonSplit spl))
                    {
                        l[i].ComponentDefID = spl.WeaponId;
                        l[i].SetComponentDefType(spl.WeaponType);
                        l[i].RefreshComponentDef();
                        if (spl.AddonId != null)
                        {
                            ChassisLocations loc = l[i].MountedLocation;
                            if (spl.NotSameLocationRequired && slotsleft(loc) <= 0)
                            {
                                foreach (ChassisLocations loc2 in AllLocs)
                                {
                                    if (slotsleft(loc2) > 0)
                                    {
                                        loc = loc2;
                                        break;
                                    }
                                }
                            }
                            MechComponentRef addon = new MechComponentRef(spl.AddonId, null, spl.AddonType, loc, -1, ComponentDamageLevel.Functional, f)
                            {
                                DataManager = m.DataManager,
                            };
                            if (spl.Link)
                            {
                                string guid = Guid.NewGuid().ToString();
                                l[i].LocalGUID = guid;
                                addon.TargetComponentGUID = guid;
                            }
                            l.Insert(i + 1, addon);
                            hasChange = true;
                        }
                        if (spl.AddSupportHardpoint) // why is LocationDef a struct???
                        {
                            LocationDef[] locs = m.Chassis.GetLocations();
                            for (int j = 0; j < locs.Length; ++j)
                            {
                                if (locs[j].Location == l[i].MountedLocation)
                                {
                                    LocationDef loc = locs[j];
                                    loc.Hardpoints = loc.Hardpoints.Append(new HardpointDef(WeaponCategoryEnumeration.GetSupport(), false)).ToArray();
                                    locs[j] = loc;
                                    hasChange = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void Register()
        {
            AutoFixer.Shared.RegisterMechFixer(Fixer);
        }
    }
}
