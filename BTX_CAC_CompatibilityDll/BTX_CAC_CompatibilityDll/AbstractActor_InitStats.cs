using BattleTech;
using HarmonyLib;
using Extended_CE;
using System.Linq;
using HBS.Collections;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch]
    class AbstractActor_InitStats
    {
        [HarmonyPatch(typeof(Mech), "InitStats")]
        [HarmonyBefore("BEX.BattleTech.Extended_CE")]
        [HarmonyPatch(typeof(Vehicle), "InitStats")]
        [HarmonyPatch(typeof(Turret), "InitStats")]
        public static void Prefix(AbstractActor __instance)
        {
            if (!__instance.Combat.IsLoadingFromSave)
            {
                __instance.StatCollection.AddStatistic("SensorLockDefense", 0f);
                __instance.StatCollection.AddStatistic("IndirectImmuneFloat", 0f);
                __instance.StatCollection.AddStatistic("DefendedByECM", 0f);
            }

            if (__instance is Mech m)
            {
                TagSet tags = m.MechDef.Chassis.ChassisTags;
                string id = m.MechDef.Description.Id;
                if (tags.Contains("CAC_C_LoadActuators"))
                {
                    ActuatorInfo ai = AEPStatic.GetActuatorInfo();
                    if (ai == null)
                    {
                        Main.Log.Log("failed to load actuator info");
                        return;
                    }
                    Main.Log.Log($"loading actuators for {id}");
                    if (tags.Contains("CAC_C_LoadActuators_No_LeftHand") && !ai.MechsWithoutLeftHand.Contains(id))
                    {
                        Main.Log.Log("no left hand");
                        ai.MechsWithoutLeftHand.Add(id);
                    }
                    if (tags.Contains("CAC_C_LoadActuators_No_RightHand") && !ai.MechsWithoutRightHand.Contains(id))
                    {
                        Main.Log.Log("no right hand");
                        ai.MechsWithoutRightHand.Add(id);
                    }
                    if (tags.Contains("CAC_C_LoadActuators_No_LeftArmLower") && !ai.MechsWithoutLeftArmLower.Contains(id))
                    {
                        Main.Log.Log("no left arm lower");
                        ai.MechsWithoutLeftArmLower.Add(id);
                    }
                    if (tags.Contains("CAC_C_LoadActuators_No_RightArmLower") && !ai.MechsWithoutRightArmLower.Contains(id))
                    {
                        Main.Log.Log("no right arm lower");
                        ai.MechsWithoutRightArmLower.Add(id);
                    }
                }
                else
                {
                    Main.Log.Log($"no actuators for {id}");
                }
            }
        }
    }
}
