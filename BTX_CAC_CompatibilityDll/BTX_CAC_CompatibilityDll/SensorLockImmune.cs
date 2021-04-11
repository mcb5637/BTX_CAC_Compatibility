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
    [HarmonyPatch(typeof(ActiveProbeSequence), "FireWave")]
    class ActiveProbeSequence_FireWave
    {
        public static bool Prefix(ActiveProbeSequence __instance, AbstractActor Target, ref int ___numWavesFired, ref float ___timeSinceLastWave)
        {
            if (Target.StatCollection.GetValue<float>("SensorLockDefense") > 0) {
                __instance.SetCamera(CameraControl.Instance.ShowSensorLockCam(Target, 2f), __instance.MessageIndex);
                CameraControl.Instance.ClearTargets();
                __instance.GetCombat().MessageCenter.PublishMessage(new FloatieMessage(__instance.owningActor.GUID, Target.GUID, "Sensor Lock blocked by ECM", FloatieMessage.MessageNature.Buff));
                ___numWavesFired++;
                ___timeSinceLastWave = 0;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SensorLockSequence), "FireWave")]
    class SensorLockSequence_FireWave
    {
        public static bool Prefix(SensorLockSequence __instance, ref int ___numWavesFired, ref float ___timeSinceLastWave)
        {
            if (__instance.Target.StatCollection.GetValue<float>("SensorLockDefense") > 0)
            {
                __instance.GetCombat().MessageCenter.PublishMessage(new FloatieMessage(__instance.owningActor.GUID, __instance.Target.GUID, "Sensor Lock blocked by ECM", FloatieMessage.MessageNature.Buff));
                ___numWavesFired++;
                ___timeSinceLastWave = 0;
                return false;
            }
            return true;
        }
    }
}
