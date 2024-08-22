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
        public string[] Use4LimitOnContractIds = new string[] { };
        public Dictionary<string, ItemCollectionReplace> ReplaceInItemCollections = new Dictionary<string, ItemCollectionReplace>();
        public Dictionary<string, WeaponAddonSplit> SplitAddons = new Dictionary<string, WeaponAddonSplit>();
        public bool MECompat = false;
        public bool FixDropslotsInOldSaves = true;
        public bool LogBlockerErrors = false;
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
    }
}
