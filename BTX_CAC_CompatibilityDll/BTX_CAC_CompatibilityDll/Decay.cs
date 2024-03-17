using BattleTech;
using CustomComponents;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    [CustomComponent("DecayMech")]
    public class CustomDecayMech : SimpleCustomChassis
    {
        public class Item
        {
            public string Id;
            public string Type;
        }
        public Item[] DecayTo = new Item[0];
    }
    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.UnreadyMech))]
    class SimGameState_UnreadyMech
    {
        public static void Postfix(SimGameState __instance, MechDef def)
        {
            if (def.Chassis.Is(out CustomDecayMech d))
            {
                foreach (var i in d.DecayTo)
                {
                    __instance.AddItemStat(i.Id, i.Type, false);
                }
                __instance.RemoveItemStat(def.Chassis.Description.Id, def.GetType(), false);
            }
        }
    }
}
