using BattleTech;
using BTRandomMechComponentUpgrader;
using FullXotlTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    internal class ComponentUpgrader
    {
        public static UpgradeList GetUpgradeList(FactionValue f)
        {
            SimGameState s = UnityGameInstance.BattleTechGame.Simulation;
            if (!s.CompanyTags.Contains("CAC_C_UpgradedComponents"))
                return null;
            string fs = f.ToString();
            if (f.IsPirate)
                fs = "DavionLocals"; // TODO
            else if (fs == "WolfsDragoons" || fs == "BlackWidowCompany")
                fs = "DavionA";
            else if (fs == "faction_Merc28") // snords
                fs = "DavionA";
            else if (fs == "ComStar")
            {
                if (s.CompanyTags.Contains("CAC_C_UpgradedComponentsCSP"))
                    fs = "ComStarPlus";
            }
            UpgradeList l = LookupUpgradeList(fs);
            if (l != null)
                return l;
            if (AEPStatic.GetXotlSettings().UnitTableReferences.TryGetValue(fs, out UnitInfo list))
            {
                return LookupUpgradeList(list.PrimaryFaction);
            }
            return null;
        }
        private static UpgradeList LookupUpgradeList(string s)
        {
            return MechProcessor.UpgradeLists.FirstOrDefault((x) => x.Factions.Contains(s));
        }

        public static void Init()
        {
            MechProcessor.GetUpgradeList = GetUpgradeList;
        }
    }
}
