using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BattleTech;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(ItemCollectionDef), "FromCSV")]
    class ItemCollectionDef_FromCSV
    {
        public static void Postfix(ItemCollectionDef __instance)
        {
            foreach (ItemCollectionDef.Entry i in __instance.Entries)
            {
                if (Main.Sett.ReplaceInItemCollections.TryGetValue(i.ID, out ItemCollectionReplace ne))
                {
                    i.ID = ne.ID;
                    if (Enum.TryParse(ne.Type, out ShopItemType t))
                        i.Type = t;
                    if (ne.Amount >= 0)
                        i.Count = ne.Amount;
                }
            }
        }
    }
}
