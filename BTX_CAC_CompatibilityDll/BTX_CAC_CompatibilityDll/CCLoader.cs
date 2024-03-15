using BattleTech;
using BattleTech.UI;
using CustomActivatableEquipment;
using CustomComponents;
using CustomUnits;
using HarmonyLib;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    [CustomComponent("RequiresLoader")]
    public class CustomRequiresLoader : SimpleCustomComponent, IMechValidate
    {
        private static bool CheckLocs(ChassisLocations c, ChassisLocations l)
        {
            if (c == l)
                return true;
            if (c == ChassisLocations.LeftArm && l == ChassisLocations.LeftTorso)
                return true;
            if (c == ChassisLocations.LeftTorso && l == ChassisLocations.LeftArm)
                return true;
            if (c == ChassisLocations.RightArm && l == ChassisLocations.RightTorso)
                return true;
            if (c == ChassisLocations.RightTorso && l == ChassisLocations.RightArm)
                return true;
            return false;
        }

        private static IEnumerable<MechComponentRef> Attachments(MechDef m, MechComponentRef r)
        {
            if (r.LocalGUID == "")
                return Array.Empty<MechComponentRef>();
            return m.Inventory.Where((i) => i.Def.Is<AddonReference>() && i.TargetComponentGUID == r.LocalGUID);
        }

        public string LoaderType;

        public void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (ValidateMechCanBeFielded(mechDef, componentRef))
                return;
            if (!errors.TryGetValue(MechValidationType.WeaponsMissing, out List<Text> err))
            {
                err = new List<Text>();
                errors[MechValidationType.WeaponsMissing] = err;
            }
            err.Add(new Text("Weapon missing Loader"));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            IEnumerable<MechComponentRef> loaders = Attachments(mechDef, componentRef).Where((r) => r.Def.ComponentTags.Contains(LoaderType));
            if (loaders.Count() != 1)
            {
                return false;
            }
            MechComponentRef l = loaders.First();
            if (l == null)
            {
                return false;
            }
            return CheckLocs(componentRef.MountedLocation, l.MountedLocation);
        }
    }
    [CustomComponent("BallisticArtilleryOnly")]
    public class CustomBallisticArtilleryOnly : SimpleCustomComponent, IMechValidate
    {
        public string ArtilleryTag;

        public void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (ValidateMechCanBeFielded(mechDef, componentRef))
                return;
            if (!errors.TryGetValue(MechValidationType.WeaponsMissing, out List<Text> err))
            {
                err = new List<Text>();
                errors[MechValidationType.WeaponsMissing] = err;
            }
            err.Add(new Text("Weapon can only be loader"));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            return !mechDef.Inventory.Any((i) =>
            {
                if (i.MountedLocation != componentRef.MountedLocation)
                    return false;
                if (i.Def.ComponentTags.Contains(ArtilleryTag))
                    return false;
                return i.Def is WeaponDef w && w.Type == WeaponType.Autocannon;
            });
        }
    }

    [HarmonyPatch(typeof(CustomHardpointsDef), nameof(CustomHardpointsDef.Apply))]
    public class CustomHardpointsDef_Apply
    {
        public static void Postfix(CustomHardpointsDef __instance, HardpointDataDef parent)
        {
            if (parent.ID == "hardpointdatadef_bullshark")
            {
                CustomHardPointsHelper.Add("chrPrfWeap_bullshark_righttorso_ac20_bh1", "chrPrfWeap_bullshark_centertorso_thumper");
            }
        }
    }
}
