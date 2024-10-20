﻿using BattleTech;
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
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Autocannon_L?AC(\\d+)_(\\d+|SPECIAL)-.+$"),
                Patch = "{\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1\r\n}\n",
            },
            new WeaponUACPattern()
            {
                Check = new Regex("^Weapon_Autocannon_C?[UR]AC(\\d+)(?:_NU|_Sa)?_(\\d+)-.+$"),
            },
            new WeaponLBXPattern()
            {
                Check = new Regex("^Weapon_Autocannon_C?LB(\\d+)X(?:_NU|_Sa)?_(\\d+)-.+$"),
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Laser_C?(?:Large|Medium|Small|Micro)LaserPulse(?:_NU|_Sa)?_(\\d+)-.+$"),
                Patch = "{\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1\r\n}\r\n",
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Laser_BinaryLaserCannon_(\\d+)-.+$"),
                Patch = "{\r\n\t\"ImprovedBallistic\": true,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"ProjectilesPerShot\": 2\r\n}\r\n",
            },
            new WeaponXPulsePattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?:Large|Medium|Small|Micro)LaserXPulse(?:_NU|_Sa)?_(\\d+)-.+$"),
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_PPC_PPC_(\\d+)-.+$"),
                Patch = "{\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_FI_ON\",\r\n\t\t\t\"UIName\": \"FI ON\",\r\n\t\t\t\"Name\": \"Field Inhibitor ON\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_FI_OFF\",\r\n\t\t\t\"UIName\": \"FI OFF\",\r\n\t\t\t\"Name\": \"Field Inhibitor OFF\",\r\n\t\t\t\"Description\": \"Disabled Field Inhibitor removes minimum range, but at the chance to misfire.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"FlatJammingChance\": 0.1,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.04,\r\n\t\t\t\"MinRange\": -90.0,\r\n\t\t\t\"AccuracyModifier\": 1.0\r\n\t\t}\r\n\t]\r\n}\r\n",
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Flamer_C?Flamer_(\\d+|SPECIAL)-.+$"),
                Patch = "{\r\n\t\"FireTerrainChance\": 0.75,\r\n\t\"FireTerrainStrength\": 1,\r\n\t\"FireOnSuccessHit\": true\r\n}\r\n",
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_MachineGun_C?MachineGun(?:Heavy|Light)?_(\\d+)-.+$"),
                Patch = "{\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"MG_Full\",\r\n\t\t\t\"UIName\": \"x5\",\r\n\t\t\t\"Name\": \"Full Salvo\",\r\n\t\t\t\"Description\": \"Fires the MG at standard speed.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"MG_Double\",\r\n\t\t\t\"UIName\": \"x10\",\r\n\t\t\t\"Name\": \"Double Salvo\",\r\n\t\t\t\"Description\": \"Fires the MG at double speed, decreasing accuracy, increasing heat, but doubles the shots per turn.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"AccuracyModifier\": 4.0,\r\n\t\t\t\"ShotsWhenFired\": 5,\r\n\t\t\t\"HeatGenerated\": 5\r\n\t\t}\r\n\t],\r\n\t\"ShotsPerAmmo\": 0.2,\r\n\t\"VolleyDivisor\": 5\r\n}\r\n",
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
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                WriteTo(targetFolder, id, Patch);
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
                string p = "{\n";
                p += $"\t\"PrefabIdentifier\": \"UAC{m.Groups[1].Value}\",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,\n";
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
                string p = $"{{\r\n\t\"PrefabIdentifier\": \"LBX{clustersize}\",\r\n\t\"VolleyDivisor\": 1,\r\n\t\"ImprovedBallistic\": true,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"Modes\": [\r\n";
                p += "\t\t{\r\n\t\t\t\"Id\": \"LBXMode_Cluster\",\r\n\t\t\t\"UIName\": \"LB-X\",\r\n\t\t\t\"Name\": \"LB-X Mode\",\r\n\t\t\t\"Description\": \"LB-X Mode fires LBX Cluster ammunition.\",\r\n\t\t\t\"isBaseMode\": true,\r\n\t\t\t\"HitGenerator\": \"Cluster\"\r\n\t\t},\r\n";
                float dmg = data.Damage * clustersize - data.Damage;
                float stab = data.Instability * clustersize - data.Instability;
                int shots = 1 - clustersize;
                p += $"\t\t{{\r\n\t\t\t\"Id\": \"LBXMode_Slug\",\r\n\t\t\t\"UIName\": \"AC\",\r\n\t\t\t\"Name\": \"AC Mode\",\r\n\t\t\t\"Description\": \"AC Mode fires AC Slug ammunition.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamagePerShot\": {dmg},\r\n\t\t\t\"Instability\": {stab},\r\n\t\t\t\"ShotsWhenFired\": {shots},\r\n\t\t\t\"WeaponEffectID\": \"WeaponEffect-Weapon_AC{clustersize}_Single\",\r\n\t\t\t\"HasShells\": false,\r\n\t\t\t\"AmmoCategory\": \"AC{clustersize}\"\r\n\t\t}}\r\n\t]\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
            }
        }

        private class WeaponXPulsePattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                float dmg = data.Damage * data.ShotsWhenFired;
                string p = $"{{\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1,\r\n\t\"Damage\": {dmg},\r\n\t\"ShotsWhenFired\": 1\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
            }
        }

        private class WeaponLRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                float minr = data.MinRange;
                int size = data.ShotsWhenFired;
                string p = $"{{\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1";
                if (minr > 0)
                {
                    p += $",\r\n\t\"Modes\": [\r\n\t\t{{\r\n\t\t\t\"Id\": \"LRM_StdLoad\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"Fired missiles arm after they leave the launcher.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}},\r\n\t\t{{\r\n\t\t\t\"Id\": \"LRM_HotLoad\",\r\n\t\t\t\"UIName\": \"HL\",\r\n\t\t\t\"Name\": \"Hot Load\",\r\n\t\t\t\"Description\": \"Missiles are armed prelaunch inside the launcher, removing minimum range but having a chance to jam.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"AccuracyModifier\": 1.0,\r\n\t\t\t\"FlatJammingChance\": 0.1,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.04,\r\n\t\t\t\"MinRange\": {minr}\r\n\t\t}}\r\n\t]\r\n}}\r\n";
                }
                else
                {
                    p += "\r\n}\r\n";
                }
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
                string p = $"{{\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"{(Streak ? "Streak" : "Individual")}\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1,\r\n\t\"Modes\": [\r\n\t\t{{\r\n\t\t\t\"Id\": \"SRM_Base\",\r\n\t\t\t\"UIName\": \"--\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}}\r\n\t]{(Streak ? ",\r\n\t\"Streak\": true" : "")}\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
                if (EnableArtemis)
                    c.AddArtemisSRM.Add(id);
            }
        }
        private class WeaponMRMPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int size = data.ShotsWhenFired;
                string p = $"{{\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Individual\",\r\n\t\"AMSHitChance\": 0.5,\r\n\t\"MissileHealth\": 2,\r\n\t\"Unguided\": true\r\n}}\r\n";
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
