using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using Harmony;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Mech), "HeatSinkCapacity", MethodType.Getter)]
    public class Mech_HeatSinkCapacity
    {
        public static void Postfix(Mech __instance, ref float __result)
        {
            __result *= __instance.StatCollection.GetValue<float>("HeatSinkCapacityMult");
        }
    }
}
