using BattleTech;
using BattleTech.UI;
using CustomComponents;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine.EventSystems;
using AccessExtension;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using System;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(MechLabPanel), nameof(MechLabPanel.OnMechLabDrop))]
    public static class MechLabPanel_OnMechLabDrop
    {
        [MethodCall(typeof(MechLabPanel), "GetLocationWidget", new System.Type[] { typeof(ChassisLocations) })]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static MechLabLocationWidget GetLocationWidget(this MechLabPanel p, ChassisLocations l)
        {
            return null;
        }

        public static void Prefix(ref bool __runOriginal, MechLabPanel __instance, PointerEventData eventData, MechLabDropTargetType addToType)
        {
            if (!__runOriginal)
                return;

            if (__instance.DragItem == null)
                return;

            if (!__instance.DragItem.ComponentRef.Def.ComponentTags.Contains("no_remove"))
                return;

            foreach (var f in new StackTrace(1).GetFrames())
            {
                if (f.GetMethod().DeclaringType == typeof(MechLabPanel_OnMechLabDrop))
                {
                    FileLog.Log("recursion!");
                    FileLog.Log(new StackTrace().ToString());
                    return;
                }
            }

            ChassisLocations loc = __instance.DragItem.MountedLocation;
            if (loc != ChassisLocations.None)
            {
                MechLabLocationWidget wid = __instance.GetLocationWidget(loc);
                wid.OnMechLabDrop(eventData, addToType);
                __instance.ShowDropErrorMessage(new Localize.Text($"{__instance.DragItem.ComponentRef.Def.Description.UIName} can not be removed!"));
                __runOriginal = false;
            }
        }
    }
    [HarmonyPatch(typeof(MechDef), "InsertFixedEquipmentIntoInventory")]
    public class MechDef_InsertFixedEquipmentIntoInventory
    {
        [HarmonyWrapSafe]
        public static void Postfix(MechDef __instance)
        {

            try
            {
                if (__instance.Chassis == null || __instance.Chassis.FixedEquipment == null)
                    return;
                List<MechComponentRef> inv = __instance.Inventory.ToList();
                int endo_slots = 0, ferro_slots = 0, combined_slots = 0;
                Dictionary<ChassisLocations, int> slots = new Dictionary<ChassisLocations, int>();

                bool has_moveables = inv.FirstOrDefault((x) => x.ComponentDefID == "Gear_EndoSteel_Movable" || x.ComponentDefID == "Gear_FerroFibrous_Movable") != null;
                if (!has_moveables)
                    SearchBEXBlockers(inv, ref endo_slots, ref ferro_slots, ref combined_slots, slots);

                endo_slots += combined_slots / 2;
                ferro_slots += combined_slots / 2 + combined_slots % 2;

                inv.RemoveAll((x) => (x.ComponentDefID.StartsWith("Gear_EndoSteel_") || x.ComponentDefID.StartsWith("Gear_FerroFibrous_")
                    || x.ComponentDefID.StartsWith("Gear_EndoFerroCombo_"))
                    && !(x.ComponentDefID == "Gear_EndoSteel_Movable" || x.ComponentDefID == "Gear_FerroFibrous_Movable"));

                if (!has_moveables)
                {
                    if (endo_slots == 0 && ferro_slots == 0
                        && (__instance.Chassis.ChassisTags.Contains("chassis_endo") || __instance.Chassis.ChassisTags.Contains("chassis_ferro")))
                    {
                        FileLog.Log($"warning: {__instance.Chassis.Description.Id} contains endo/ferro tags but no blockers");
                    }

                    HandleBlockers(__instance, inv, endo_slots, slots, "chassis_endo", "EndoStructureBlocker", "Gear_EndoSteel_Movable");
                    HandleBlockers(__instance, inv, ferro_slots, slots, "chassis_ferro", "FerroStructureBlocker", "Gear_FerroFibrous_Movable");
                }

                __instance.SetInvNoCheck(inv.ToArray());
            }
            catch (Exception e)
            {
                FileLog.Log(e.ToString());
                throw;
            }
        }

        private static void HandleBlockers(MechDef __instance, List<MechComponentRef> inv, int num_slots, Dictionary<ChassisLocations, int> slots, string tag, string category, string component)
        {
            ChassisDef c = __instance.Chassis;
            if (num_slots > 0)
            {
                if (!c.ChassisTags.Contains(tag))
                    FileLog.Log($"Warning: {c.Description.Id} contains {category} blockers but no {tag} tag");
                if (c.GetComponents<ChassisCategory>().FirstOrDefault((cat) => cat.CategoryID == category) == null)
                {
                    ChassisCategory cat = new ChassisCategory()
                    {
                        CategoryID = category,
                        LocationLimits = new Dictionary<ChassisLocations, CategoryLimit>(),
                    };
                    cat.LocationLimits[ChassisLocations.All] = new CategoryLimit(num_slots, num_slots, false);
                    c.AddComponent(cat);
                }
                for (int i = 0; i < num_slots; i++)
                {
                    ChassisLocations l = GetLocationForComponent(slots);
                    if (l == ChassisLocations.None)
                        FileLog.Log($"error: loc none in {__instance.Description.Id}");
                    MechComponentRef comp = new MechComponentRef(component, Guid.NewGuid().ToString(), ComponentType.Upgrade, l)
                    {
                        DataManager = __instance.DataManager
                    };
                    comp.RefreshComponentDef();
                    inv.Add(comp);
                }
            }
        }

        private static void SearchBEXBlockers(List<MechComponentRef> inv, ref int endo_slots, ref int ferro_slots, ref int combined_slots, Dictionary<ChassisLocations, int> slots)
        {
            foreach (MechComponentRef item in inv)
            {
                if (item.IsFixed)
                {
                    if (item.ComponentDefID.StartsWith("Gear_EndoSteel_") && item.ComponentDefID != "Gear_EndoSteel_Movable")
                    {
                        item.RefreshComponentDef();
                        endo_slots += item.Def.InventorySize;
                        AddSlot(item.MountedLocation, item.Def.InventorySize, slots);
                    }
                    if (item.ComponentDefID.StartsWith("Gear_FerroFibrous_") && item.ComponentDefID != "Gear_FerroFibrous_Movable")
                    {
                        item.RefreshComponentDef();
                        ferro_slots += item.Def.InventorySize;
                        AddSlot(item.MountedLocation, item.Def.InventorySize, slots);
                    }
                    if (item.ComponentDefID.StartsWith("Gear_EndoFerroCombo_"))
                    {
                        item.RefreshComponentDef();
                        combined_slots += item.Def.InventorySize;
                        AddSlot(item.MountedLocation, item.Def.InventorySize, slots);
                    }
                }
            }
        }

        private static ChassisLocations GetLocationForComponent(Dictionary<ChassisLocations, int> slots)
        {
            foreach (ChassisLocations loc in slots.Keys)
            {
                if (slots[loc] <= 1)
                {
                    slots.Remove(loc);
                }
                else
                {
                    slots[loc]--;
                }
                return loc;
            }
            return ChassisLocations.None;
        }

        private static void AddSlot(ChassisLocations loc, int s, Dictionary<ChassisLocations, int> slots)
        {
            if (!slots.ContainsKey(loc))
                slots[loc] = s;
            else
                slots[loc] += s;
        }
    }
}
