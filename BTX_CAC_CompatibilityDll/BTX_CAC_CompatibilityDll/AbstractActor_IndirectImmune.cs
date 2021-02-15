using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    class AbstractActor_IndirectImmune
    {
        public static void Postfix(AbstractActor __instance, ref bool __result)
        {
            if (__instance.StatCollection.GetValue<float>("IndirectImmuneFloat") > 0)
                __result = true;
        }

        public static void Patch(HarmonyInstance h)
        {
            HarmonyMethod p = new HarmonyMethod(AccessTools.Method(typeof(AbstractActor_IndirectImmune), "Postfix"));
            h.Patch(AccessTools.Property(typeof(Mech), "HasIndirectFireImmunity").GetMethod, null, p, null);
            h.Patch(AccessTools.Property(typeof(Vehicle), "HasIndirectFireImmunity").GetMethod, null, p, null);
            h.Patch(AccessTools.Property(typeof(Turret), "HasIndirectFireImmunity").GetMethod, null, p, null);
        }
    }
}
