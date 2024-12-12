using AccessExtension;
using BattleTech;
using CustAmmoCategories;
using CustomUnits;
using Extended_CE;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch]
    public class CU2ComponentFix
    {
        public static void Patch(Harmony h)
        {
            try
            {
                FixMech_InitStats_PatchOrder(h);
            }
            catch (Exception e)
            {
                Main.Log.LogError(e.ToString());
            }
        }

        private static void FixMech_InitStats_PatchOrder(Harmony h)
        {
            MethodInfo target = AccessTools.Method(typeof(Mech), "InitStats");
            Patches patches = Harmony.GetPatchInfo(target);
            List<Patch> needsReplacement = new List<Patch>();
            foreach (Patch patch in patches.Prefixes)
            {
                if (patch.owner == "BEX.BattleTech.Extended_CE" || patch.owner == "BEX.BattleTech.MechQuirks")
                    needsReplacement.Add(patch);
            }
            foreach (Patch patch in needsReplacement)
            {
                var p = new HarmonyMethod(patch.PatchMethod)
                {
                    before = new string[] { "io.mission.customdeploy" }
                };
                h.Unpatch(target, patch.PatchMethod);
                h.Patch(target, p);
            }
        }

        [MethodCall(typeof(BTComponents.Mech_InitStats), "AddOurComponent")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AddOurComponent(Mech mech, string componentId, ChassisLocations location)
        {

        }

        [HarmonyBefore("io.mission.customdeploy")]
        [HarmonyPatch(typeof(Mech), "InitStats")]
        [HarmonyPrefix]
        public static void Mech_InitStats_Prefix(Mech __instance)
        {
            if (__instance is FakeVehicleMech)
            {
                AddOurComponent(__instance, "Gear_BEX_MotiveSystem", FakeVehicleLocationConvertHelper.chassisLocationFromVehicleLocation(VehicleChassisLocations.Front));
                AddOurComponent(__instance, "Gear_BEX_MotiveSystem", FakeVehicleLocationConvertHelper.chassisLocationFromVehicleLocation(VehicleChassisLocations.Left));
                AddOurComponent(__instance, "Gear_BEX_MotiveSystem", FakeVehicleLocationConvertHelper.chassisLocationFromVehicleLocation(VehicleChassisLocations.Right));
                AddOurComponent(__instance, "Gear_BEX_MotiveSystem", FakeVehicleLocationConvertHelper.chassisLocationFromVehicleLocation(VehicleChassisLocations.Rear));
            }
        }

        [HarmonyPatch(typeof(Mech), nameof(Mech.TakeWeaponDamage))]
        [HarmonyPostfix]
        public static void Mech_TakeWeaponDamage_Postfix(Mech __instance, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon)
        {
            if (!(__instance is FakeVehicleMech))
                return;
            VehicleDef vehicleDef = __instance.MechDef.toVehicleDef(__instance.MechDef.DataManager);
            if (vehicleDef.VehicleTags.Contains("unit_vtol"))
                return;
            ChassisLocations chassisloc = (ChassisLocations)hitLocation;
            VehicleChassisLocations vehloc = chassisloc.toFakeVehicleChassis();
            if (!(vehloc == VehicleChassisLocations.Front || vehloc == VehicleChassisLocations.Right || vehloc == VehicleChassisLocations.Left || vehloc == VehicleChassisLocations.Rear))
                return;
            if (weapon.DamagePerShot < 0)
                return;
            if (__instance.IsLocationDestroyed(chassisloc))
                return;
            float critchance = 1.0f - __instance.GetCurrentStructure(chassisloc) / __instance.GetMaxStructure(chassisloc);
            critchance = Mathf.Max(critchance, __instance.Combat.Constants.ResolutionConstants.MinCritChance);
            critchance *= __instance.Combat.CritChance.GetCritMultiplier(__instance, weapon, false);
            if (vehloc == VehicleChassisLocations.Front)
                critchance *= 0.5f;
            else if (vehloc == VehicleChassisLocations.Rear)
                critchance *= 0.75f;
            if (vehicleDef.VehicleTags.Contains("unit_tracks"))
                critchance *= 0.5f;
            else if (vehicleDef.VehicleTags.Contains("unit_hover"))
                critchance *= 0.75f;
            if (weapon.WeaponSubType >= WeaponSubType.LRM5 && weapon.WeaponSubType <= WeaponSubType.LRM20)
                critchance *= 0.2f;
            float[] random = __instance.Combat.AttackDirector.GetRandomFromCache(hitInfo, 1);
            if (random[0] <= critchance)
            {
                MechComponent tocrit = __instance.allComponents.FirstOrDefault((c) => c.Location == hitLocation && c.Description.Id.StartsWith("Gear_BEX_MotiveSystem"));
                if (tocrit != null)
                {
                    if (__instance.GameRep != null)
                    {
                        if (weapon.weaponRep != null && weapon.weaponRep.HasWeaponEffect)
                            WwiseManager.SetSwitch(weapon.weaponRep.WeaponEffect.weaponImpactType, __instance.GameRep.audioObject);
                        else
                            WwiseManager.SetSwitch(AudioSwitch_weapon_type.laser_medium, __instance.GameRep.audioObject);
                        WwiseManager.SetSwitch(AudioSwitch_surface_type.mech_critical_hit, __instance.GameRep.audioObject);
                        WwiseManager.PostEvent(AudioEventList_impact.impact_weapon, __instance.GameRep.audioObject, null, null);
                        WwiseManager.PostEvent(AudioEventList_explosion.explosion_small, __instance.GameRep.audioObject, null, null);
                        if (__instance.team.LocalPlayerControlsTeam)
                            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "critical_hit_friendly ", null, null);
                        else if (!__instance.team.IsFriendly(__instance.Combat.LocalPlayerTeam))
                            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "critical_hit_enemy", null, null);
                    }
                    tocrit.DamageComponent(hitInfo, ComponentDamageLevel.Penalized, true);
                }
            }
        }
    }
}
