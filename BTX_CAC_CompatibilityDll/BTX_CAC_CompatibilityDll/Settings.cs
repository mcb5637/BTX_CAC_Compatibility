using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    class Settings
    {
        public bool LogLevelLog = true;
        public bool Use4LimitOnAllStoryMissions = false;
        public string[] Use4LimitOnContractIds = new string[] { };
        public bool MECompat = false;
        public bool FixDropslotsInOldSaves = true;
        public bool LogBlockerErrors = false;
        public int MaxNumberOfPlayerUnitsOverride = 12;
    }

    class ItemCollectionReplace
    {
        public string ID = null;
        public string Type = null;
        public int Amount = -1;
    }

    class WeaponAddonSplit
    {
        public string WeaponId = null;
        public string AddonId = null;
        public ComponentType AddonType = ComponentType.Upgrade;
        public ComponentType WeaponType = ComponentType.Weapon;
        public bool Link = true;
        public bool NotSameLocationRequired = false;
        public bool AddSupportHardpoint = false;
    }
}
