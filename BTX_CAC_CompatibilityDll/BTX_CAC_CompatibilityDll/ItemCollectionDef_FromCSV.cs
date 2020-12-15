using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
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
                if (Main.Sett.ReplaceInItemCollections.TryGetValue(i.ID, out string ne))
                {
                    i.ID = ne;
                }
            }
        }
    }
}
