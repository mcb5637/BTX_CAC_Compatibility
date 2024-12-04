using BattleTech;
using BattleTech.Data;
using CustAmmoCategories;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RectTransform;

namespace BTX_CAC_CompatibilityDll
{
#if DEBUG
    internal class AutoPatchGenerator
    {
        public static void Generate(DataManager m)
        {
            string targetfolder = Path.Combine(Main.Directory, "automerge");
            if (Directory.Exists(targetfolder))
                Directory.Delete(targetfolder, true);
            IdCollector c = new IdCollector();
            GenerateWeapons(m, targetfolder, c);
            GenerateAdvMerges(m, targetfolder, c);

            {
                File.WriteAllText(Path.Combine(targetfolder, "itemcollectionreplace.json"), JsonConvert.SerializeObject(c.ICReplace, Formatting.Indented));
                File.WriteAllText(Path.Combine(targetfolder, "addonsplit.json"), JsonConvert.SerializeObject(c.Splits, Formatting.Indented));
            }
        }

        private static void GenerateAdvMerges(DataManager m, string targetfolder, IdCollector c)
        {
            string amfolder = Path.Combine(targetfolder, "adv");
            Directory.CreateDirectory(amfolder);
            {
                c.AddArtemisLRM.Sort();
                string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
                p += c.AddArtemisLRM.Join((s) => $"\"{s}\"", ",\r\n\t\t");
                p += "\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{\r\n\t\t\t\"JSONPath\": \"$.ComponentTags.items\",\r\n\t\t\t\"Action\": \"ArrayAdd\",\r\n\t\t\t\"Value\": \"artemis_lrm_attachable\"\r\n\t\t}\r\n\t]\r\n}\r\n";

                File.WriteAllText(Path.Combine(amfolder, "artemis_lrm_enable.json"), p);
            }
            {
                c.AddArtemisSRM.Sort();
                string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
                p += c.AddArtemisSRM.Join((s) => $"\"{s}\"", ",\r\n\t\t");
                p += "\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{\r\n\t\t\t\"JSONPath\": \"$.ComponentTags.items\",\r\n\t\t\t\"Action\": \"ArrayAdd\",\r\n\t\t\t\"Value\": \"artemis_srm_attachable\"\r\n\t\t}\r\n\t]\r\n}\r\n";

                File.WriteAllText(Path.Combine(amfolder, "artemis_srm_enable.json"), p);
            }
            {
                c.AddPPCCap.Sort();
                string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
                p += c.AddPPCCap.Join((s) => $"\"{s}\"", ",\r\n\t\t");
                p += "\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{\r\n\t\t\t\"JSONPath\": \"$.ComponentTags.items\",\r\n\t\t\t\"Action\": \"ArrayAdd\",\r\n\t\t\t\"Value\": \"ppc_capacitor_attachable\"\r\n\t\t}\r\n\t]\r\n}\r\n";

                File.WriteAllText(Path.Combine(amfolder, "ppc_cap_enable.json"), p);
            }
            {
                c.AddPPCCapSnub.Sort();
                string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
                p += c.AddPPCCapSnub.Join((s) => $"\"{s}\"", ",\r\n\t\t");
                p += "\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{\r\n\t\t\t\"JSONPath\": \"$.ComponentTags.items\",\r\n\t\t\t\"Action\": \"ArrayAdd\",\r\n\t\t\t\"Value\": \"ppc_capacitor_attachable_snub\"\r\n\t\t}\r\n\t]\r\n}\r\n";

                File.WriteAllText(Path.Combine(amfolder, "ppc_cap_snub_enable.json"), p);
            }
        }

        private static void GenerateWeapons(DataManager m, string targetfolder, IdCollector c)
        {
            string wepfolder = Path.Combine(targetfolder, "weapon");
            Directory.CreateDirectory(wepfolder);
            foreach (KeyValuePair<string, WeaponDef> kv in m.WeaponDefs)
            {
                if (kv.Key != kv.Value.Description.Id)
                    throw new InvalidDataException("id missmatch: " + kv.Key);
                foreach (Pattern<WeaponDef> p in WeaponPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, wepfolder, kv.Key, c);
                    break;
                }
            }
        }

        private static readonly Pattern<WeaponDef>[] WeaponPatterns = new Pattern<WeaponDef>[] {
            new WeaponACPattern()
            {
                Check = new Regex("^Weapon_Autocannon_L?AC(\\d+)_(\\d+|SPECIAL)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Gauss_(?:C|Heavy|Light)?Gauss(?:_NU|_Sa|Magshot)?_(\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Gauss_Silver_Bullet_Gauss_(\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"FireDelayMultiplier\": 0,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"BallisticDamagePerPallet\": true,\r\n\t\"ShotsWhenFired\": 1,\r\n\t\"ProjectilesPerShot\": 12,\r\n\t\"Damage\": 108,\r\n\t\"Instability\": 60,\r\n\t\"ProjectileScale\": {\r\n\t\t\"x\": 0.2,\r\n\t\t\"y\": 0.2,\r\n\t\t\"z\": 0.2\r\n\t}\r\n}",
                Details = false,
                Damage = false,
            },
            new WeaponUACPattern()
            {
                Check = new Regex("^Weapon_Autocannon_C?[UR]AC(\\d+)(?:_NU|_Sa)?_(\\d+)-.+$"),
            },
            new WeaponLBXPattern()
            {
                Check = new Regex("^Weapon_Autocannon_C?LB(\\d+)X(?:_NU|_Sa)?_(\\d+)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?:Large|Medium|Small|Micro)LaserX?Pulse(?:_NU|_Sa)?_(\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1\r\n}\r\n",
                Details = true,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?:Large|Medium|Small|Micro)LaserER(?:_NU|_Sa)?_(\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_BinaryLaserCannon_(\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"ProjectilesPerShot\": 2\r\n}\r\n",
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_PPC_PPC_(\\d+)-.+$"),
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_FI_OFF\",\r\n\t\t\t\"UIName\": \"FI OFF\",\r\n\t\t\t\"Name\": \"Field Inhibitor OFF\",\r\n\t\t\t\"Description\": \"Disabled Field Inhibitor removes minimum range, but at the chance to misfire.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"FlatJammingChance\": 0.1,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.04,\r\n\t\t\t\"MinRange\": -90.0,\r\n\t\t\t\"AccuracyModifier\": 1.0\r\n\t\t}\r\n\t]\r\n}\r\n",
                AddToList = (x) => x.AddPPCCap,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_PPC_C?PPC(?:ER|Heavy)(?:_NU|_Sa)?_(\\d+)-.+$"),
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}\r\n\t]\r\n}\r\n",
                AddToList = (x) => x.AddPPCCap,
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_PPC_PPCSnub_(\\d+)-.+$"),
                Patch = "{\r\n\t\"MinRange\": 0,\r\n\t\"MaxRange\": 450,\r\n\t\"RangeSplit\": [\r\n\t\t270,\r\n\t\t390,\r\n\t\t450\r\n\t],\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"DistantVariance\": 0.5,\r\n\t\"DamageFalloffStartDistance\": 390,\r\n\t\"DamageFalloffEndDistance\": 450,\r\n\t\"DistantVarianceReversed\": false,\r\n\t\"RangedDmgFalloffType\": \"Linear\",\r\n\t\"isDamageVariation\": true,\r\n\t\"isStabilityVariation\": true,\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"SPPC_STD\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"Snub PPC fires normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"SPPC_FL\",\r\n\t\t\t\"UIName\": \"FL\",\r\n\t\t\t\"Name\": \"Focusing Lens\",\r\n\t\t\t\"Description\": \"The additional magnetic Focusing Lens allows to focus all particles into one projectile, concentrating the infliced damage to one location, at the cost of slighly increased heat generation.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"ShotsWhenFired\": -4,\r\n\t\t\t\"HeatGenerated\": 5,\r\n\t\t\t\"DamageMultiplier\": 5,\r\n\t\t\t\"InstabilityMultiplier\": 5,\r\n\t\t\t\"WeaponEffectID\": \"WeaponEffect-Weapon_PPC\"\r\n\t\t}\r\n\t]\r\n}",
                AddToList = (x) => x.AddPPCCapSnub,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Flamer_C?Flamer_(\\d+|SPECIAL)-.+$"),
                ExtraData = ",\r\n\t\"FireTerrainChance\": 0.75,\r\n\t\"FireTerrainStrength\": 1,\r\n\t\"FireOnSuccessHit\": true\r\n}\r\n",
                Heat = true,
                Details = true,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_MachineGun_C?MachineGun(?:Heavy|Light)?_(\\d+)-.+$"),
                Details = true,
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"MG_Full\",\r\n\t\t\t\"UIName\": \"x5\",\r\n\t\t\t\"Name\": \"Full Salvo\",\r\n\t\t\t\"Description\": \"Fires the MG at standard speed.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"MG_Double\",\r\n\t\t\t\"UIName\": \"x10\",\r\n\t\t\t\"Name\": \"Double Salvo\",\r\n\t\t\t\"Description\": \"Fires the MG at double speed, decreasing accuracy, increasing heat, but doubles the shots per turn.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"AccuracyModifier\": 4.0,\r\n\t\t\t\"ShotsWhenFired\": 5,\r\n\t\t\t\"HeatGenerated\": 5\r\n\t\t}\r\n\t],\r\n\t\"ShotsPerAmmo\": 0.2,\r\n\t\"VolleyDivisor\": 5\r\n}\r\n",
            },
            new WeaponLRMPattern()
            {
                Check = new Regex("^Weapon_LRM_C?N?LRM(\\d+)_(\\d+)-.+$"),
                EnableArtemis = true,
            },
            new WeaponLRMPattern()
            {
                Check = new Regex("^Weapon_ELRM_C?ELRM(\\d+)_(\\d+)-.+$"),
                EnableArtemis = false,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Thunderbolt_Thunderbolt(\\d+)_(\\d+)-.+$"),
                Details = true,
                ExtraData = "\r\n}\r\n",
            },
            new DeprecatedPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_LRM_(C?)A(N?LRM\\d+)_(\\d+-.+)$"),
                Id = new string[]{ "Gear_Addon_Artemis4" },
                Type = "Upgrade",
                Addon = "Gear_Addon_Artemis4",
                Main = "Weapon_LRM_{0}{1}_{2}",
            },
            new DeprecatedPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_LRM_(C?)DFLRM(\\d+)_(\\d+-.+)$"),
                Id = new string[]{ "Ammo_AmmunitionBox_Generic_LRM_DF_Half", "Ammo_AmmunitionBox_Generic_LRM_DF" },
                Div = 11,
                Type = "AmmunitionBox",
                Main = "Weapon_LRM_{0}LRM{1}_{2}",
            },
            new WeaponSRMPattern()
            {
                Check = new Regex("^Weapon_SRM_C?SRM(\\d+)_(\\d+-.+|OneShot)$"),
                EnableArtemis = true,
                Streak = false,
            },
            new WeaponSRMPattern()
            {
                Check = new Regex("^Weapon_SRM_C?SSRM(\\d+)_(\\d+-.+|OneShot)$"),
                EnableArtemis = false,
                Streak = true,
            },
            new DeprecatedPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_SRM_(C?)A(N?SRM\\d+)_(\\d+-.+|OneShot)$"),
                Id = new string[]{ "Gear_Addon_Artemis4" },
                Type = "Upgrade",
                Addon = "Gear_Addon_Artemis4",
                Main = "Weapon_SRM_{0}{1}_{2}",
            },
            new DeprecatedPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_SRM_(C?)DFSRM(\\d+)_(\\d+-.+|OneShot)$"),
                Id = new string[]{ "Ammo_AmmunitionBox_Generic_SRM_DF_Half", "Ammo_AmmunitionBox_Generic_SRM_DF" },
                Div = 4,
                Type = "AmmunitionBox",
                Main = "Weapon_SRM_{0}SRM{1}_{2}",
            },
            new DeprecatedPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Inferno_Inferno(\\d+)_(\\d+)-.+$"),
                Id = new string[]{ "Ammo_AmmunitionBox_Generic_SRM_Inferno_Half", "Ammo_AmmunitionBox_Generic_SRM_Inferno" },
                Div = 2,
                Type = "AmmunitionBox",
            },
            new WeaponMRMPattern()
            {
                Check = new Regex("^Weapon_(?:MRM|RL)_(?:MRM|RL)(\\d+)_(\\d+-.+)$"),
            },
        };

        private class IdCollector
        {
            public readonly List<string> AddArtemisLRM = new List<string>();
            public readonly List<string> AddArtemisSRM = new List<string>();
            public readonly List<string> AddPPCCap = new List<string>();
            public readonly List<string> AddPPCCapSnub = new List<string>();
            public readonly Dictionary<string, ItemCollectionReplace> ICReplace = new Dictionary<string, ItemCollectionReplace>();
            public readonly Dictionary<string, WeaponAddonSplit> Splits = new Dictionary<string, WeaponAddonSplit>();
        }

        private abstract class Pattern<T>
        {
            public Regex Check;
            public abstract void Generate(T data, Match m, string targetFolder, string id, IdCollector c);
            public void WriteTo(string targetfolder, string id, string merge)
            {
                File.WriteAllText(Path.ChangeExtension(Path.Combine(targetfolder, id), "json"), merge);
            }
        }


        private class StaticPatchPattern<T> : Pattern<T>
        {
            public string Patch;
            public Func<IdCollector, List<string>> AddToList = null;
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                WriteTo(targetFolder, id, Patch);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
            }
        }
        private class DeprecatedPatchPattern<T> : Pattern<T>
        {
            public int Amount = -1;
            public string Type = null;
            public string[] Id;
            public int Div = 0;
            public string Addon = null;
            public string Main = null;
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                WriteTo(targetFolder, id, "{\r\n\t\"Custom\": {\r\n\t\t\"Flags\": [\r\n\t\t\t\"invalid\"\r\n\t\t]\r\n\t}\r\n}\r\n");
                string i;
                if (Div > 0)
                {
                    int j = int.Parse(m.Groups[2].Value);
                    i = Id[j / Div];
                }
                else
                {
                    i = Id[0];
                }
                c.ICReplace[id] = new ItemCollectionReplace()
                {
                    Amount = Amount,
                    Type = Type,
                    ID = i,
                };
                if (Main != null)
                {
                    c.Splits[id] = new WeaponAddonSplit()
                    {
                        AddonId = Addon,
                        WeaponId = string.Format(Main, m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value)
                    };
                }
            }
        }

        private class WeaponForwardingPattern : Pattern<WeaponDef>
        {
            public string ExtraData;
            public bool Details = false;
            public bool Heat = false;
            public bool Damage = true;
            public Func<IdCollector, List<string>> AddToList = null;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = Forward(data, Details, Heat, Damage);
                p += ExtraData;
                WriteTo(targetFolder, id, p);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
            }

            public static string Forward(WeaponDef data, bool details, bool heat, bool damage = true)
            {
                string p = $"{{\r\n\t\"MinRange\": {data.MinRange},\r\n\t\"MaxRange\": {data.MaxRange},\r\n\t\"RangeSplit\": [\r\n\t\t{data.RangeSplit[0]},\r\n\t\t{data.RangeSplit[1]},\r\n\t\t{data.RangeSplit[2]}\r\n\t],\r\n\t\"HeatGenerated\": {data.HeatGenerated}";
                if (damage)
                    p += $",\r\n\t\"Damage\": {data.Damage},\r\n\t\"Instability\": {data.Instability}";
                p += $",\r\n\t\"RefireModifier\": {data.RefireModifier},\r\n\t\"AccuracyModifier\": {data.AccuracyModifier}";
                if (heat)
                    p += $",\r\n\t\"HeatDamage\": {data.HeatDamage}";
                if (details)
                    p += $",\r\n\t\"Description\": {{\r\n\t\t\"Details\": {JsonConvert.ToString(data.Description.Details)}\r\n\t}}";
                return p;
            }
        }
        private class WeaponACPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, false, false);
                p += ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,";
                p += $"\r\n\t\"RestrictedAmmo\": [\r\n\t\t\"Ammunition_LB{m.Groups[1].Value}X\"\r\n\t]\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
            }
        }
        private class WeaponUACPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int heat = data.HeatGenerated / data.ShotsWhenFired;
                int shotSelection = 2;
                if (id.Contains("RAC"))
                    shotSelection = 6;
                string tag = data.ComponentTags.FirstOrDefault((s) => s.StartsWith("BEX_RF_"));
                if (tag != null)
                {
                    if (int.TryParse(tag.Substring(7), out int r))
                        shotSelection = r;
                }
                int uacrapidfireacc = 3; //TODO read from settings
                string p = WeaponForwardingPattern.Forward(data, false, false);
                p += $",\r\n\t\"PrefabIdentifier\": \"UAC{m.Groups[1].Value}\",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,\r\n";
                p += $"\t\"RestrictedAmmo\": [\r\n\t\t\"Ammunition_AC{m.Groups[1].Value}AP\",\r\n\t\t\"Ammunition_AC{m.Groups[1].Value}Precision\",\r\n\t\t\"Ammunition_AC{m.Groups[1].Value}Tracer\",\r\n\t\t\"Ammunition_LB{m.Groups[1].Value}X\"\r\n\t],\r\n";
                p += "\t\"Custom\": {\r\n\t\t\"Clustering\": {\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 7,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}\r\n\t\t\t],\r\n\t\t\t\"Base\": 0.6333333,\r\n\t\t\t\"UAC\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"Shots\": 2,\r\n\t\t\t\t\t\"Base\": 0.7083333\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"Shots\": 3,\r\n\t\t\t\t\t\"Base\": 0.6666667\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"Shots\": 4,\r\n\t\t\t\t\t\"Base\": 0.6597222\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t}\r\n\t},\r\n";
                p += "\t\"Modes\": [\n";
                for (int mi = 1; mi <= shotSelection; mi++)
                {
                    int acc = 0;
                    if (mi > 3)
                        acc = uacrapidfireacc + 1;
                    else if (mi > 1)
                        acc = uacrapidfireacc;
                    bool basemode = data.ShotsWhenFired == mi;
                    int sfired = mi - data.ShotsWhenFired;
                    int heatmod = heat * mi - data.HeatGenerated;
                    string name;
                    string desc;
                    if (shotSelection == 2)
                    {
                        if (mi == 1)
                        {
                            name = "AC Mode";
                            desc = "AC Mode fires only one projectile, but at increased accuracy.";
                        }
                        else
                        {
                            name = "UAC Mode";
                            desc = "UAC Mode fires 2 projectiles.";
                        }
                    }
                    else
                    {
                        name = $"{mi} Shot Mode";
                        desc = $"Fires {mi} projectiles";
                        if (mi > 3)
                            desc += " with a high accuaracy penalty";
                        else if (mi > 1)
                            desc += " with an accuaracy penalty";
                        desc += ".";
                    }
                    p += $"\t\t{{\r\n\t\t\t\"Id\": \"UACMode_{mi}\",\r\n\t\t\t\"UIName\": \"x{mi}\",\r\n\t\t\t\"Name\": \"{name}\",\r\n\t\t\t\"Description\": \"{desc}\",\r\n\t\t\t\"isBaseMode\": {basemode.ToString().ToLower()},\r\n\t\t\t\"ShotsWhenFired\": {sfired},\r\n\t\t\t\"AccuracyModifier\": {acc},\r\n\t\t\t\"HeatGenerated\": {heatmod}\r\n\t\t}}";
                    if (mi < shotSelection)
                        p += ",";
                    p += "\n";
                }
                p += "\t]\r\n}\n";
                WriteTo(targetFolder, id, p);
            }
        }
        private class WeaponLBXPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int clustersize = int.Parse(m.Groups[1].Value);
                if (clustersize != data.ShotsWhenFired)
                    throw new InvalidDataException("lbx size missmatch " + id);
                string p = WeaponForwardingPattern.Forward(data, false, false);
                p += $",\r\n\t\"PrefabIdentifier\": \"LBX{clustersize}\",\t\n\t\"ShotsWhenFired\": {data.ShotsWhenFired},\r\n\t\"VolleyDivisor\": 1,\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,\r\n";
                p += $"\t\"Custom\": {{\r\n\t\t\"Clustering\": {{\r\n\t\t\t\"Base\": {(clustersize <= 2 ? 0.7083333f : 0.6333333f)}\r\n\t\t}}\r\n\t}},\r\n";
                p += "\t\"Modes\": [\r\n";
                float dmg = data.Damage * clustersize - data.Damage;
                float stab = data.Instability * clustersize - data.Instability;
                int shots = 1 - clustersize;
                p += $"\t\t{{\r\n\t\t\t\"Id\": \"LBXMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"\",\r\n\t\t\t\"isBaseMode\": true,\r\n\t\t\t\"DamagePerShot\": {dmg},\r\n\t\t\t\"Instability\": {stab},\r\n\t\t\t\"ShotsWhenFired\": {shots},\r\n\t\t\t\"WeaponEffectID\": \"WeaponEffect-Weapon_AC{clustersize}_Single\",\r\n\t\t\t\"HasShells\": false,\r\n\t\t\t\"AmmoCategory\": \"AC{clustersize}\"\r\n\t\t}}\r\n\t]\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
            }
        }

        private class WeaponLRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                if (id == "Weapon_LRM_LRM15_1-DeltaBoT")
                    return;
                float minr = data.MinRange;
                int size = data.ShotsWhenFired;
                string p = WeaponForwardingPattern.Forward(data, true, false);
                p += $",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1";
                p += ",\r\n\t\"Custom\" : {\r\n\t\t\"Clustering\": {\r\n\t\t\t\"Base\": 0.6333333,\r\n\t\t\t\"DeadfireBase\": 0.54320985,\r\n\t\t\t\"ArtemisBase\": 0.76666665,\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 6,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 10,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t}\r\n\t}";
                p += ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"LRM_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}";
                if (minr > 0)
                {
                    p += $",\r\n\t\t{{\r\n\t\t\t\"Id\": \"LRM_HotLoad\",\r\n\t\t\t\"UIName\": \"HL\",\r\n\t\t\t\"Name\": \"Hot Load\",\r\n\t\t\t\"Description\": \"Missiles are armed inside the launcher instead of after launching, removing minimum range but having a chance to jam.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"AccuracyModifier\": 1.0,\r\n\t\t\t\"FlatJammingChance\": 0.1,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.04,\r\n\t\t\t\"MinRange\": {-minr}\r\n\t\t}}";
                }
                p += "\r\n\t]\r\n}\r\n";
                WriteTo(targetFolder, id, p);
                if (EnableArtemis)
                    c.AddArtemisLRM.Add(id);
            }
        }
        private class WeaponSRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis, Streak;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int size = data.ShotsWhenFired;
                string p = WeaponForwardingPattern.Forward(data, true, false);
                p += $",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"{(Streak ? "Streak" : "Individual")}\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1,\r\n";
                if (data.ShotsWhenFired >= 6)
                    p += "\t\"Custom\" : {\r\n\t\t\"Clustering\": {\r\n\t\t\t\"Base\": 0.6333333,\r\n\t\t\t\"DeadfireBase\": 0.54320985,\r\n\t\t\t\"ArtemisBase\": 0.76666665,\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 6,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 10,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t}\r\n\t},\r\n";
                else if (data.ShotsWhenFired >= 4)
                    p += "\t\"Custom\" : {\r\n\t\t\"Clustering\": {\r\n\t\t\t\"Base\": 0.6597222,\r\n\t\t\t\"DeadfireBase\": 0.568287,\r\n\t\t\t\"ArtemisBase\": 0.7962963,\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 6,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 10,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t}\r\n\t},\r\n";
                else
                    p += "\t\"Custom\" : {\r\n\t\t\"Clustering\": {\r\n\t\t\t\"Base\": 0.7083333,\r\n\t\t\t\"DeadfireBase\": 0.599537,\r\n\t\t\t\"ArtemisBase\": 0.8611111,\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 6,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 10,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t}\r\n\t},\r\n";
                p += $"\t\"Modes\": [\r\n\t\t{{\r\n\t\t\t\"Id\": \"SRM_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}}\r\n\t]{(Streak ? ",\r\n\t\"Streak\": true" : "")}\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
                if (EnableArtemis)
                    c.AddArtemisSRM.Add(id);
            }
        }
        private class WeaponMRMPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, true, false);
                int size = data.ShotsWhenFired;
                p += $",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Individual\",\r\n\t\"AMSHitChance\": 0.5,\r\n\t\"MissileHealth\": 2,\r\n\t\"Unguided\": true";
                p += ",\r\n\t\"Custom\" : {\r\n\t\t\"Clustering\": {\r\n\t\t\t\"Base\": 0.54320985,\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 6,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"GunnerySkill\": 10,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t}\r\n\t}\r\n}\r\n";
                WriteTo(targetFolder, id, p);
            }
        }

    }

    [HarmonyPatch(typeof(SimGameState), "SetSimRoomState")]
    internal class SimGameState_SetSimRoomState
    {
        public static void Prefix(SimGameState __instance, DropshipLocation state)
        {
            if (state == DropshipLocation.CMD_CENTER && Input.GetKey(KeyCode.LeftShift))
            {
                try
                {
                    AutoPatchGenerator.Generate(__instance.DataManager);
                }
                catch (Exception e)
                {
                    FileLog.Log(e.ToString());
                }
            }
        }
    }
#endif
}
