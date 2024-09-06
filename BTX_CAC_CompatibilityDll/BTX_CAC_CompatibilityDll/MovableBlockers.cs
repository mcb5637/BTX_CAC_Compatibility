﻿using BattleTech;
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
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;

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
                Dictionary<ChassisLocations, int> slot_locations = new Dictionary<ChassisLocations, int>();

                bool has_moveables = inv.FirstOrDefault((x) => x.ComponentDefID == "Gear_EndoSteel_Movable" || x.ComponentDefID == "Gear_FerroFibrous_Movable") != null;
                if (!has_moveables)
                    SearchBEXBlockers(__instance.Chassis.FixedEquipment, ref endo_slots, ref ferro_slots, ref combined_slots, slot_locations);

                endo_slots += combined_slots / 2;
                ferro_slots += combined_slots / 2 + combined_slots % 2;

                inv.RemoveAll((x) => (x.ComponentDefID.StartsWith("Gear_EndoSteel_") || x.ComponentDefID.StartsWith("Gear_FerroFibrous_")
                    || x.ComponentDefID.StartsWith("Gear_EndoFerroCombo_"))
                    && !(x.ComponentDefID == "Gear_EndoSteel_Movable" || x.ComponentDefID == "Gear_FerroFibrous_Movable"));

                if (!has_moveables)
                {
                    if (endo_slots == 0 && ferro_slots == 0 && Main.Sett.LogBlockerErrors
                        && (__instance.Chassis.ChassisTags.Contains("chassis_endo") || __instance.Chassis.ChassisTags.Contains("chassis_ferro")))
                    {
                        Main.Log.LogWarning($"warning: {__instance.Chassis.Description.Id} contains endo/ferro tags but no blockers");
                    }

                    HandleBlockers(__instance, inv, endo_slots, slot_locations, "chassis_endo", "EndoStructureBlocker", "Gear_EndoSteel_Movable");
                    HandleBlockers(__instance, inv, ferro_slots, slot_locations, "chassis_ferro", "FerroStructureBlocker", "Gear_FerroFibrous_Movable");
                }

                __instance.SetInvNoCheck(inv.ToArray());
            }
            catch (Exception e)
            {
                Main.Log.LogException(e);
                throw;
            }
        }

        private static void HandleBlockers(MechDef __instance, List<MechComponentRef> inv, int num_slots, Dictionary<ChassisLocations, int> locations, string tag, string category, string component)
        {
            ChassisDef c = __instance.Chassis;
            if (num_slots > 0)
            {
                if (Main.Sett.LogBlockerErrors && !c.ChassisTags.Contains(tag))
                    Main.Log.LogWarning($"Warning: {c.Description.Id} contains {category} blockers but no {tag} tag");
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
                    ChassisLocations l = GetLocationForComponent(locations);
                    if (l == ChassisLocations.None)
                    {
                        Main.Log.LogError($"error: loc none in {__instance.Description.Id}");
                        break;
                    }
                    MechComponentRef comp = new MechComponentRef(component, Guid.NewGuid().ToString(), ComponentType.Upgrade, l)
                    {
                        DataManager = __instance.DataManager
                    };
                    comp.RefreshComponentDef();
                    inv.Add(comp);
                }
            }
        }

        private static void SearchBEXBlockers(IEnumerable<MechComponentRef> inv, ref int endo_slots, ref int ferro_slots, ref int combined_slots, Dictionary<ChassisLocations, int> locations)
        {
            foreach (MechComponentRef item in inv)
            {
                if (item.IsFixed)
                {
                    if (item.ComponentDefID.StartsWith("Gear_EndoSteel_") && item.ComponentDefID != "Gear_EndoSteel_Movable")
                    {
                        item.RefreshComponentDef();
                        endo_slots += item.Def.InventorySize;
                        AddSlot(item.MountedLocation, item.Def.InventorySize, locations);
                    }
                    if (item.ComponentDefID.StartsWith("Gear_FerroFibrous_") && item.ComponentDefID != "Gear_FerroFibrous_Movable")
                    {
                        item.RefreshComponentDef();
                        ferro_slots += item.Def.InventorySize;
                        AddSlot(item.MountedLocation, item.Def.InventorySize, locations);
                    }
                    if (item.ComponentDefID.StartsWith("Gear_EndoFerroCombo_"))
                    {
                        item.RefreshComponentDef();
                        combined_slots += item.Def.InventorySize;
                        AddSlot(item.MountedLocation, item.Def.InventorySize, locations);
                    }
                }
            }
        }

        private static ChassisLocations GetLocationForComponent(Dictionary<ChassisLocations, int> locations)
        {
            foreach (ChassisLocations loc in locations.Keys)
            {
                if (locations[loc] <= 1)
                {
                    locations.Remove(loc);
                }
                else
                {
                    locations[loc]--;
                }
                return loc;
            }
            return ChassisLocations.None;
        }

        private static void AddSlot(ChassisLocations loc, int s, Dictionary<ChassisLocations, int> locations)
        {
            if (!locations.ContainsKey(loc))
                locations[loc] = s;
            else
                locations[loc] += s;
        }
    }
    [HarmonyPatch]
    public class SimGameState_StripMech
    {
        private static bool IsFixedMod(MechComponentRef r)
        {
            return r.IsFixed || r.Def.ComponentTags.Contains("no_remove");
        }
        [HarmonyPatch(typeof(SimGameState), "StripMech")]
        [HarmonyPatch("CustomComponents.Patches.MechLabLocationWidget_StripEquipment_Patch", "Prefix")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo search = AccessTools.Property(typeof(BaseComponentRef), nameof(BaseComponentRef.IsFixed)).GetMethod;
            foreach (CodeInstruction i in instructions)
            {
                if ((i.opcode == OpCodes.Callvirt || i.opcode == OpCodes.Call) && (i.operand as MethodInfo) == search)
                    i.operand = AccessTools.Method(typeof(SimGameState_StripMech), nameof(IsFixedMod));
                yield return i;
            }
        }
    }
    [HarmonyPatch(typeof(SimGameState), "CreateComponentInstallWorkOrder")]
    [HarmonyAfter("io.github.denadan.CustomComponents")]
    public class SimGameState_CreateComponentInstallWorkOrder
    {
        public static void Postfix(MechComponentRef mechComponent, WorkOrderEntry_InstallComponent __result)
        {
            if (mechComponent.Def.ComponentTags.Contains("no_remove"))
            {
                __result.SetCost(1);
                __result.SetCBillCost(0);
            }
        }
    }
    [HarmonyPatch(typeof(MechLabLocationWidget), nameof(MechLabLocationWidget.SetData))]
    public static class MechLabLocationWidget_SetData
    {
        [FieldGet(typeof(MechLabLocationWidget), "maxSlots")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetMaxSlots(this MechLabLocationWidget i)
        {
            return 0;
        }

        public static IEnumerable<Transform> GetChildren(this Transform t)
        {
            foreach (Transform c in t)
                yield return c;
        }

        internal static void EnableLayout(GameObject gameObject)
        {
            {
                var component = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
                component.ignoreLayout = false;
                component.enabled = true;
            }

            {
                var component = gameObject.GetComponent<HorizontalLayoutGroup>();
                if (component != null)
                {
                    component.childForceExpandHeight = false;
                    component.childAlignment = TextAnchor.UpperCenter;
                    component.enabled = true;
                }
            }

            {
                var component = gameObject.GetComponent<VerticalLayoutGroup>();
                if (component != null)
                {
                    component.enabled = true;
                    component.childForceExpandHeight = false;
                    component.childForceExpandWidth = false;
                }
            }

            {
                var component = gameObject.GetComponent<GridLayoutGroup>();
                if (component != null)
                {
                    component.enabled = true;
                }
            }

            {
                var component = gameObject.GetComponent<ContentSizeFitter>();
                if (component != null)
                {
                    component.enabled = true;
                }
            }
        }

        public static void Postfix(MechLabLocationWidget __instance, LocationLoadoutDef loadout)
        {
            Transform layout_slots = __instance.transform.Find("layout_slots");
            List<Transform> slots = layout_slots.GetChildren().Where(x => x.name.StartsWith("slot")) .ToList();
            GameObject template = slots[0].gameObject;
            int slotTarget = __instance.GetMaxSlots();
            while (slots.Count > slotTarget)
            {
                Transform last = slots[slots.Count - 1];
                slots.RemoveAt(slots.Count - 1);
                last.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(last);
            }
            while (slots.Count < slotTarget)
            {
                GameObject n = UnityEngine.Object.Instantiate(template, layout_slots);
                n.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + slots.Count);
                n.name = $"slot ({slots.Count})";
                n.SetActive(true);
                slots.Add(n.transform);
            }
            EnableLayout(layout_slots.gameObject);
            EnableLayout(__instance.gameObject);

            GameObject vertical = __instance.transform.parent.gameObject;
            if (loadout.Location == ChassisLocations.LeftTorso || loadout.Location == ChassisLocations.RightTorso)
            {
                __instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, 444, 0);
            }
            else if (loadout.Location == ChassisLocations.LeftLeg || loadout.Location == ChassisLocations.RightLeg)
            {
                __instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, -138, 0);
            }
            else if (loadout.Location == ChassisLocations.LeftArm || loadout.Location == ChassisLocations.RightArm)
            {
            }
            else
            {
                GameObject go = __instance.transform.parent.gameObject;
                EnableLayout(go);
                ContentSizeFitter component = go.GetComponent<ContentSizeFitter>() ?? go.AddComponent<ContentSizeFitter>();
                component.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                component.enabled = true;
                {
                    var clg = go.GetComponent<VerticalLayoutGroup>();
                    clg.padding = new RectOffset(0, 0, 0, 0);
                    clg.spacing = 56;
                }
            }
            {
                Transform t = __instance.transform.parent.parent;
                t.localScale = new Vector3(1, 0.95f, 1);
                t.localPosition = new Vector3(300, 0, 0);
            }
        }
    }
}
