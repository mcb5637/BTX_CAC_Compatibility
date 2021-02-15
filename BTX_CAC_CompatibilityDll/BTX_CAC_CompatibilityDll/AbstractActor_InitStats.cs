using BattleTech;
using Harmony;

namespace BTX_CAC_CompatibilityDll
{
    //[HarmonyPatch(typeof(Mech), "InitStats")]
    class AbstractActor_InitStats
    {
        public static void Prefix(AbstractActor __instance)
        {
            if (!__instance.Combat.IsLoadingFromSave)
            {
                __instance.StatCollection.AddStatistic("SensorLockDefense", 0f);
                __instance.StatCollection.AddStatistic("IndirectImmuneFloat", 0f);
            }
        }

        public static void Patch(HarmonyInstance h)
        {
            HarmonyMethod p = new HarmonyMethod(AccessTools.Method(typeof(AbstractActor_InitStats), "Prefix"));
            h.Patch(AccessTools.DeclaredMethod(typeof(Mech), "InitStats"), p, null, null);
            h.Patch(AccessTools.DeclaredMethod(typeof(Vehicle), "InitStats"), p, null, null);
            h.Patch(AccessTools.DeclaredMethod(typeof(Turret), "InitStats"), p, null, null);
        }
    }
}
