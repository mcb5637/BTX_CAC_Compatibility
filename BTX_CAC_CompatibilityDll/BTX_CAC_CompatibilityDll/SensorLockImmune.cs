using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(SelectionStateActiveProbe), "CalcPossibleTargets")]
    class SelectionStateActiveProbe_CalcPossibleTargets
    {
        public static void Postfix(ref List<ICombatant> __result)
        {
            __result = __result.Where((c) => !(c is AbstractActor) || (c as AbstractActor).StatCollection.GetValue<float>("SensorLockDefense") <= 0).ToList();
        }
    }

    [HarmonyPatch(typeof(SelectionStateSensorLock), "CalcPossibleTargets")]
    class SelectionStateSensorLock_CalcPossibleTargets
    {
        public static void Postfix(ref List<ICombatant> __result)
        {
            __result = __result.Where((c) => !(c is AbstractActor) || (c as AbstractActor).StatCollection.GetValue<float>("SensorLockDefense") <= 0).ToList();
        }
    }

    [HarmonyPatch(typeof(Mech), "InitStats")]
    class AbstractActor_InitStats
    {
        public static void Prefix(AbstractActor __instance)
        {
            if (!__instance.Combat.IsLoadingFromSave)
            {
                __instance.StatCollection.AddStatistic("SensorLockDefense", 0f);
            }
        }
    }
}
