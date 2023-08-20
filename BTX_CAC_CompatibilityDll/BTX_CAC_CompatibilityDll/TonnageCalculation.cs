using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using HarmonyLib;
using Localize;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BTX_CAC_CompatibilityDll
{
    static class TonnageCalculation
    {
        private static int Calc(ChassisDef c, long armorPoints, IEnumerable<MechComponentRef> inventory)
        {
            int kg = (int)(c.InitialTonnage * 1000.0f);
            long kgperpoint;
            if (c.ChassisTags.Contains("chassis_ferro"))
            {
                if (c.ChassisTags.Contains("chassis_clan"))
                    kgperpoint = 960;
                else
                    kgperpoint = 896;
            }
            else
            {
                kgperpoint = 800;
            }
            kg += (int)(armorPoints * 10L / kgperpoint);
            foreach (var i in inventory)
            {
                kg += (int)(i.Def.Tonnage * 1000.0f);
            }
            if (kg / 10 == (int)(c.Tonnage * 100.0f)) // ignore everything BT will not display
                return (int)(c.Tonnage * 1000.0f);
            return kg;
        }
        private static long GetArmor(float i)
        {
            return (long)(i * 1000.0f);
        }
        public static int CalculateWeightKG(this MechDef m)
        {
            long armorPoints = GetArmor(m.Head.AssignedArmor) + GetArmor(m.CenterTorso.AssignedArmor) + GetArmor(m.CenterTorso.AssignedRearArmor)
                + GetArmor(m.LeftTorso.AssignedArmor) + GetArmor(m.LeftTorso.AssignedRearArmor)
                + GetArmor(m.RightTorso.AssignedArmor) + GetArmor(m.RightTorso.AssignedRearArmor)
                + GetArmor(m.LeftArm.AssignedArmor) + GetArmor(m.RightArm.AssignedArmor)
                + GetArmor(m.LeftLeg.AssignedArmor) + GetArmor(m.RightLeg.AssignedArmor);
            return Calc(m.Chassis, armorPoints, m.Inventory);
        }
        public static int CalculateWeightKG(this MechLabPanel w)
        {
            if (w.activeMechDef == null)
                return 0;
            long armorPoints = GetArmor(w.headWidget.currentArmor) + GetArmor(w.centerTorsoWidget.currentArmor) + GetArmor(w.centerTorsoWidget.currentRearArmor)
                + GetArmor(w.leftTorsoWidget.currentArmor) + GetArmor(w.leftTorsoWidget.currentRearArmor)
                + GetArmor(w.rightTorsoWidget.currentArmor) + GetArmor(w.rightTorsoWidget.currentRearArmor)
                + GetArmor(w.leftArmWidget.currentArmor) + GetArmor(w.rightArmWidget.currentArmor)
                + GetArmor(w.leftLegWidget.currentArmor) + GetArmor(w.rightLegWidget.currentArmor);
            return Calc(w.activeMechDef.Chassis, armorPoints, w.activeMechInventory);
        }
    }

    [HarmonyPatch(typeof(MechStatisticsRules), nameof(MechStatisticsRules.CalculateTonnage))]
    public class MechStatisticsRules_CalculateTonnage
    {
        public static void Postfix(MechDef mechDef, ref float currentValue, ref float maxValue)
        {
            maxValue = mechDef.Chassis.Tonnage;
            currentValue = mechDef.CalculateWeightKG() / 1000.0f;
        }
    }
    [HarmonyPatch(typeof(MechLabMechInfoWidget), "CalculateTonnage")]
    public class MechLabMechInfoWidget_CalculateTonnage
    {
        public static void Postfix(MechLabMechInfoWidget __instance, MechLabPanel ___mechLab, LocalizableText ___totalTonnage, UIColorRefTracker ___totalTonnageColor,
            LocalizableText ___remainingTonnage, UIColorRefTracker ___remainingTonnageColor)
        {
            if (___mechLab.activeMechDef != null)
            {
                int kg = ___mechLab.CalculateWeightKG();
                int chassiskg = (int)(___mechLab.activeMechDef.Chassis.Tonnage * 1000.0f);
                int remaining = chassiskg - kg;
                __instance.currentTonnage = kg / 1000.0f;
                ___totalTonnage.SetText("{0:0.##} / {1}", __instance.currentTonnage, ___mechLab.activeMechDef.Chassis.Tonnage);
                ___totalTonnageColor.SetUIColor(remaining < 0 ? UIColor.Red : UIColor.WhiteHalf);
                if (remaining < 0)
                {
                    float t = -remaining / 1000.0f;
                    ___remainingTonnage.SetText("{0:0.##} ton{1} overweight", t, remaining == -1000 ? "" : "s");
                }
                else
                {
                    ___remainingTonnage.SetText("{0:0.##} ton{1} remaining", remaining / 1000.0f, remaining == 1000 ? "" : "s");
                }
                ___remainingTonnageColor.SetUIColor(remaining < 0 ? UIColor.Red : (remaining<=500 ? UIColor.Gold : UIColor.White));
            }
        }
    }
    [HarmonyPatch(typeof(MechValidationRules), nameof(MechValidationRules.ValidateMechTonnage))]
    public class MechValidationRules_ValidateMechTonnage
    {
        public static bool Prefix(MechDef mechDef, ref Dictionary<MechValidationType, List<Text>> errorMessages)
        {
            int max = (int)(mechDef.Chassis.Tonnage * 1000.0f);
            int actual = mechDef.CalculateWeightKG();
            if (actual > max)
            {
                errorMessages[MechValidationType.Overweight].Add(new Text("OVERWEIGHT: 'Mech weight exceeds maximum tonnage for the Chassis"));
            }
            else if (actual < max - 500)
            {
                errorMessages[MechValidationType.Underweight].Add(new Text("UNDERWEIGHT: 'Mech has unused tonnage"));
            }

            return false;
        }
    }
}
