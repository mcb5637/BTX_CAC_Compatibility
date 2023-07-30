using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(MechDef), nameof(MechDef.FromJSON))]
    class MechDef_FromJson
    {
        public static void Postfix(MechDef __instance)
        {
            List<MechComponentRef> l = __instance.Inventory.ToList();
            for (int i = 0; i < l.Count; i++)
            {
                if (Main.Sett.SplitAddons.TryGetValue(l[i].ComponentDefID, out WeaponAddonSplit spl))
                {
                    string guid = Guid.NewGuid().ToString();
                    l[i].LocalGUID = guid;
                    l[i].ComponentDefID = spl.WeaponId;
                    MechComponentRef addon = new MechComponentRef(spl.AddonId, null, ComponentType.Upgrade, l[i].MountedLocation)
                    {
                        TargetComponentGUID = guid
                    };
                    l.Insert(i + 1, addon);
                }
            }
            __instance.SetInventory(l.ToArray());
        }
    }
}
