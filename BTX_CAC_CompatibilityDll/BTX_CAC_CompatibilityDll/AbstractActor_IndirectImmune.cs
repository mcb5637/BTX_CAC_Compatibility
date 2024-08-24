using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch]
    class AbstractActor_IndirectImmune
    {
        [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.HasIndirectFireImmunity), MethodType.Getter)]
        public static void Postfix(AbstractActor __instance, ref bool __result)
        {
            if (__instance.StatCollection.GetValue<float>("IndirectImmuneFloat") > 0)
                __result = true;
        }
    }
}
