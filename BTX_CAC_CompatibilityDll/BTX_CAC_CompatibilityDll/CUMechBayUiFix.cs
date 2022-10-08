using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(MechBayDragDropSlot), "OnAddItem")]
        public class MechBayDragDropSlot_OnAddItem
        {
            public static void Prefix(IMechLabDraggableItem item)
            {
                MechBayMechUnitElement e = (MechBayMechUnitElement)item;
                var f = Traverse.Create(e).Field("cRTrans");
                if (f.GetValue() == null)
                {
                    f.SetValue(e.GetComponent<RectTransform>());
                }
            }
        }
}
