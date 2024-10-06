using BattleTech;
using BattleTech.Data;
using CustAmmoCategories;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
#if DEBUG
    internal class AutoPatchGenerator
    {
        public static void Generate(DataManager m)
        {
            GenerateWeapons(m);
        }

        private static void GenerateWeapons(DataManager m)
        {
            string targetfolder = Path.Combine(Main.Directory, "automerge", "weapon");
            if (Directory.Exists(targetfolder))
                Directory.Delete(targetfolder, true);
            Directory.CreateDirectory(targetfolder);
            foreach (KeyValuePair<string, WeaponDef> kv in m.WeaponDefs)
            {
                if (kv.Key != kv.Value.Description.Id)
                    throw new InvalidDataException("id missmatch: " + kv.Key);
                foreach (Pattern<WeaponDef> p in WeaponPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, targetfolder, kv.Key);
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
        };

        private abstract class Pattern<T>
        {
            public Regex Check;
            public abstract void Generate(T data, Match m, string targetFolder, string id);
            public void WriteTo(string targetfolder, string id, string merge)
            {
                File.WriteAllText(Path.ChangeExtension(Path.Combine(targetfolder, id), "json"), merge);
            }
        }


        private class StaticPatchPattern<T> : Pattern<T>
        {
            public string Patch;
            public override void Generate(T data, Match m, string targetFolder, string id)
            {
                WriteTo(targetFolder, id, Patch);
            }
        }
        private class WeaponUACPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id)
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
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id)
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
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id)
            {
                float dmg = data.Damage * data.ShotsWhenFired;
                string p = $"{{\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1,\r\n\t\"Damage\": {dmg},\r\n\t\"ShotsWhenFired\": 1\r\n}}\r\n";
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
