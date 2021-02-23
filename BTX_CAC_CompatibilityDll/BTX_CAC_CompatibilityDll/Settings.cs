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
        public bool MECompat = false;
    }

    class ItemCollectionReplace
    {
        public string ID = null;
        public string Type = null;
    }
}
