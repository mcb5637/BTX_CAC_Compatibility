using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(MechDef), nameof(MechDef.FromJSON))]
    internal class MechDef_FromJson
    {
        public static Dictionary<string, WeaponAddonSplit> Splits = new Dictionary<string, WeaponAddonSplit>();

        public static void Postfix(MechDef __instance)
        {
            List<MechComponentRef> l = __instance.Inventory.ToList();
            for (int i = 0; i < l.Count; i++)
            {
                if (Splits.TryGetValue(l[i].ComponentDefID, out WeaponAddonSplit spl))
                {
                    l[i].ComponentDefID = spl.WeaponId;
                    if (spl.AddonId != null)
                    {
                        string guid = Guid.NewGuid().ToString();
                        l[i].LocalGUID = guid;
                        MechComponentRef addon = new MechComponentRef(spl.AddonId, null, ComponentType.Upgrade, l[i].MountedLocation)
                        {
                            TargetComponentGUID = guid
                        };
                        l.Insert(i + 1, addon);
                    }
                }
            }
            __instance.SetInventory(l.ToArray());
        }
    }
}
