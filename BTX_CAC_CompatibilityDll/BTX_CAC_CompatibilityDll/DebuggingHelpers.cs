using BattleTech;
using CustomUnits;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static BattleTech.SimGameBattleSimulator;

namespace BTX_CAC_CompatibilityDll
{
#if false
    [HarmonyPatch]
    public class DebuggingHelpers
    {
        private static void RequestItem(SimGameState s, string id, Action<ItemCollectionDef> callback, BattleTechResourceType resourceType)
        {
            FileLog.Log($"requesting itemcollection {id}");
            s.RequestItem(id, (ItemCollectionDef x) =>
            {
                if (x == null)
                    FileLog.Log($"Failed to load itemcollection {id}");
                callback(x);
            }, resourceType);
        }
        [HarmonyPatch(typeof(ItemCollectionResultGenerator), "ProcessQueuedReferenceCollections")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ItemCollectionResultGenerator_ProcessQueuedReferenceCollections(IEnumerable<CodeInstruction> i)
        {
            MethodInfo s = AccessTools.Method(typeof(SimGameState), nameof(SimGameState.RequestItem)).MakeGenericMethod(typeof(ItemCollectionDef));
            MethodInfo r = AccessTools.Method(typeof(DebuggingHelpers), nameof(RequestItem));
            foreach (var c in i)
            {
                if ((c.opcode == OpCodes.Callvirt || c.opcode == OpCodes.Call) && (c.operand as MethodInfo)?.Name == s.Name)
                    c.operand = r;
                yield return c;
            }
        }

        [HarmonyPatch(typeof(ItemCollectionResultGenerator), "InsertItemCollectionEntry")]
        [HarmonyPrefix]
        public static void ItemCollectionResultGenerator_InsertItemCollectionEntry(SimGameState ____simGame, ItemCollectionDef.Entry entry)
        {
            ShopDefItem i = new ShopDefItem(entry);
            if (i.Type == ShopItemType.MechPart || i.Type == ShopItemType.Mech)
            {
                if (!____simGame.DataManager.MechDefs.TryGet(i.ID, out var _))
                {
                    FileLog.Log($"invalid mech {i.ID}");
                }
            }
            else
            {
                try
                {
                    if (____simGame.GetMechComponentDefFromShopItemType(i.Type, i.ID) == null)
                        FileLog.Log($"invalid {i.Type} {i.ID} null");
                }
                catch (Exception e)
                {
                    FileLog.Log($"invalid {i.Type} {i.ID} {e}");
                }
            }
        }

        [HarmonyPatch(typeof(ItemCollectionResultGenerator), "GenerateItemCollection")]
        [HarmonyPrefix]
        public static void ItemCollectionResultGenerator_GenerateItemCollection(ItemCollectionDef collection, ref Action<ItemCollectionResult> cb, string parentGUID)
        {
            FileLog.Log($"generating itemcollection {collection.ID} for {parentGUID.SafeToString()}");
            var cbo = cb;
            cb = (x) =>
            {
                FileLog.Log($"done generating itemcollection {collection.ID} for {parentGUID.SafeToString()} {x.GUID} {x.parentGUID.SafeToString()}");
                cbo(x);
            };
        }
        [HarmonyPatch(typeof(Shop), "OnItemsCollected")]
        [HarmonyPrefix]
        public static void Shop_OnItemsCollected(Shop __instance, ItemCollectionResult result)
        {
            FileLog.Log($"shop items generated {__instance.ThisShopType} {result.GUID} {result.parentGUID.SafeToString()}");
        }

        //[HarmonyPatch(typeof(SimGameState), "SetSimRoomState")]
        //[HarmonyPrefix]
        //public static void SimGameState_SetSimRoomState(SimGameState __instance, DropshipLocation state)
        //{
        //    if (state == DropshipLocation.CMD_CENTER && Input.GetKey(KeyCode.LeftShift))
        //    {
        //        if (!__instance.DataManager.ItemCollectionDefs.TryGet("itemCollection_Weapons_Flamers_rare", out var ic))
        //        {
        //            FileLog.Log($"failed");
        //            return;
        //        }
        //        foreach (ItemCollectionDef.Entry i in ic.Entries)
        //        {
        //            FileLog.Log($"item {i.ID} {i.Type} {i.Count} {i.Weight}");
        //        }
        //    }
        //}
    }
#endif
}
