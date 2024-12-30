using BattleTech;
using BattleTech.Data;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
#if DEBUG
    internal class AutoPatchGenerator
    {
        internal enum ComponentOrder : int
        {
            Invalid = -1,
            Fixed = 0,
            Blocker,

            Artillery,
            ArtilleryLoader,
            BombBay,
            BombAP,
            BombHE,
            BombI,
            BombEmpty,

            GaussHeavy = 50,
            Gauss,
            GaussLight,
            GaussSB,

            AC20U,
            AC20LB,
            AC20,

            AC10U,
            AC10LB,
            AC10,

            AC5R,
            AC5U,
            AC5LB,
            AC5,
            AC5Li,

            AC2R,
            AC2U,
            AC2LB,
            AC2,
            AC2Li,


            HPPC,
            ERPPC,
            SPPC,
            PPC,
            PPCCap,

            Plasma,

            COILL,
            BLaser,
            HLLaser,
            XPLLaser,
            PLLaser,
            ERLLaser,
            LLaser,

            COILM,
            HMLaser,
            XPMLaser,
            PMLaser,
            ERMLaser,
            MLaser,

            TAG,



            ATM12,
            ATM9,
            ATM6,
            ATM3,

            ELRM20,
            NLRM20,
            LRM20,

            ELRM15,
            NLRM15,
            LRM15,

            ELRM10,
            NLRM10,
            LRM10,

            ELRM5,
            NLRM5,
            LRM5,

            SSRM6,
            SRM6,
            SSRM4,
            SRM4,
            SSRM2,
            SRM2,

            Artemis,

            MRM40,
            MRM30,
            MRM20,
            MRM10,

            RL20,
            RL15,
            RL10,

            INARC,
            NARC,


            GaussMagshot,
            AMS,
            HMG,
            MG,
            LMG,
            Flamer,

            COILS,
            HSLaser,
            XPSLaser,
            PSLaser,
            ERSLaser,
            SLaser,

            PMiLaser,
            ERMiLaser,

            AmmoArt = 150,

            AmmoHGauss,
            AmmoGauss,
            AmmoLGauss,
            AmmoGaussSB,

            AmmoAC20,
            AmmoAC20AP,
            AmmoAC20Tr,
            AmmoAC20P,
            AmmoAC20LB,

            AmmoAC10,
            AmmoAC10AP,
            AmmoAC10Tr,
            AmmoAC10P,
            AmmoAC10LB,

            AmmoAC5,
            AmmoAC5AP,
            AmmoAC5Tr,
            AmmoAC5P,
            AmmoAC5LB,

            AmmoAC2,
            AmmoAC2AP,
            AmmoAC2Tr,
            AmmoAC2P,
            AmmoAC2LB,



            AmmoPlasma,


            AmmoATM,
            AmmoATMER,
            AmmoATMHE,

            AmmoELRM,
            AmmoLRM,
            AmmoLRMDF,

            AmmoSRM,
            AmmoSRMInf,
            AmmoSRMDF,

            AmmoMRM,

            AmmoINARC,
            AmmoINARCEX,
            AmmoINARCH,

            AmmoNARC,
            AmmoNARCEX,


            AmmoGaussMagshot,
            AmmoHMG,
            AmmoMG,


            Actuator = 250,
            CockpitMod,
            C3,
            Gyro,
            TAC,
            TTS,

            APod,

            HSinkD,
            HSink,
            HSinkX,
            HBank,
            HEx,
            HCPod,

            JJ,
        }

        public static void Generate(DataManager m)
        {
            string targetfolder = Path.Combine(Main.Directory, "automerge");
            if (Directory.Exists(targetfolder))
                Directory.Delete(targetfolder, true);
            IdCollector c = new IdCollector();
            GenerateWeapons(m, targetfolder, c);
            GenerateAmmoBoxes(m, targetfolder, c);
            GenerateJJComponents(m, targetfolder, c);
            GenerateHSComponents(m, targetfolder, c);
            GenerateUpgradeComponents(m, targetfolder, c);
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
            {
                c.AddNarcCompatible.Sort();
                string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
                p += c.AddNarcCompatible.Join((s) => $"\"{s}\"", ",\r\n\t\t");
                p += "\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{\r\n\t\t\t\"JSONPath\": \"$.ComponentTags.items\",\r\n\t\t\t\"Action\": \"ArrayAdd\",\r\n\t\t\t\"Value\": \"component_usesnarc\"\r\n\t\t}\r\n\t]\r\n}\r\n";

                File.WriteAllText(Path.Combine(amfolder, "narc_enable.json"), p);
            }
            foreach (KeyValuePair<ComponentOrder, List<string>> kv in c.Order)
            {
                if (kv.Key == ComponentOrder.Invalid)
                {
                    FileLog.Log("invalid order: " + kv.Value.Join());
                    continue;
                }
                kv.Value.Sort();

                string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
                p += kv.Value.Join((s) => $"\"{s}\"", ",\r\n\t\t");
                p += $"\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{{\r\n\t\t\t\"JSONPath\": \"$\",\r\n\t\t\t\"Action\": \"ObjectMerge\",\r\n\t\t\t\"Value\": {{\r\n\t\t\t\t\"Custom\": {{\r\n\t\t\t\t\t\"Sorter\": {(int)kv.Key}\r\n\t\t\t\t}}\r\n\t\t\t}}\r\n\t\t}}\r\n\t]\r\n}}";

                File.WriteAllText(Path.Combine(amfolder, $"order_{kv.Key}.json"), p);
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
                bool found = false;
                foreach (Pattern<WeaponDef> p in WeaponPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, wepfolder, kv.Key, c);
                    found = true;
                    break;
                }
                if (!found)
                    FileLog.Log($"weapon {kv.Key} does not have a pattern");
            }
        }

        private static readonly Pattern<WeaponDef>[] WeaponPatterns = new Pattern<WeaponDef>[] {
            new WeaponACPattern()
            {
                Check = new Regex("^Weapon_Autocannon_(?<li>L?)AC(?<size>\\d+)_(?:\\d+|SPECIAL)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Gauss_(?<hl>C|Heavy|Light)?Gauss(?<mag>_NU|_Sa|Magshot)?_(?:\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["mag"].Value == "Magshot")
                        return ComponentOrder.GaussMagshot;
                    if (m.Groups["hl"].Value == "Heavy")
                        return ComponentOrder.GaussHeavy;
                    if (m.Groups["hl"].Value == "Light")
                        return ComponentOrder.GaussLight;
                    return ComponentOrder.Gauss;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Gauss_Silver_Bullet_Gauss_(\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"FireDelayMultiplier\": 0,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"BallisticDamagePerPallet\": true,\r\n\t\"ShotsWhenFired\": 1,\r\n\t\"ProjectilesPerShot\": 12,\r\n\t\"Damage\": 108,\r\n\t\"Instability\": 60,\r\n\t\"ProjectileScale\": {\r\n\t\t\"x\": 0.2,\r\n\t\t\"y\": 0.2,\r\n\t\t\"z\": 0.2\r\n\t}\r\n}",
                Details = false,
                Damage = false,
                Order = (m) => ComponentOrder.GaussSB,
            },
            new WeaponUACPattern()
            {
                Check = new Regex("^Weapon_Autocannon_C?(?<ur>[UR])AC(?<size>\\d+)(?:_NU|_Sa)?_(?:\\d+)-.+$"),
            },
            new WeaponLBXPattern()
            {
                Check = new Regex("^Weapon_Autocannon_C?LB(?<size>\\d+)X(?:_NU|_Sa)?_(?:\\d+)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?<size>Large|Medium|Small|Micro)LaserPulse(?:_NU|_Sa)?_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1\r\n}\r\n",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.PLLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.PMLaser;
                    if (m.Groups["size"].Value == "Small")
                        return ComponentOrder.PSLaser;
                    return ComponentOrder.PMiLaser;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?<size>Large|Medium|Small)LaserXPulse(?:_NU|_Sa)?_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1\r\n}\r\n",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.XPLLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.XPMLaser;
                    return ComponentOrder.XPSLaser;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?<size>Large|Medium|Small|Micro)LaserER(?:_NU|_Sa)?_(?:\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.ERLLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.ERMLaser;
                    if (m.Groups["size"].Value == "Small")
                        return ComponentOrder.ERSLaser;
                    return ComponentOrder.ERMiLaser;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?<size>Large|Medium|Small)Laser?_(?:-?\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.LLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.MLaser;
                    return ComponentOrder.SLaser;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_C?(?<size>Large|Medium|Small)LaserHeavy?_(?:-?\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.HLLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.HMLaser;
                    return ComponentOrder.HSLaser;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_COIL_COIL-(?<size>S|M|L)_.+$"),
                ExtraData = "\r\n}\r\n",
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.COILL;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.COILM;
                    return ComponentOrder.COILS;
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Laser_BinaryLaserCannon_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"ProjectilesPerShot\": 2\r\n}\r\n",
                Order = (m) => ComponentOrder.BLaser,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_PPC_PPC_(?:-?\\d+)-.+$"),
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_FI_OFF\",\r\n\t\t\t\"UIName\": \"FI OFF\",\r\n\t\t\t\"Name\": \"Field Inhibitor OFF\",\r\n\t\t\t\"Description\": \"Disabled Field Inhibitor removes minimum range, but at the chance to misfire.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"FlatJammingChance\": 0.1,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.04,\r\n\t\t\t\"MinRange\": -90.0,\r\n\t\t\t\"AccuracyModifier\": 1.0\r\n\t\t}\r\n\t]\r\n}\r\n",
                AddToList = (x) => x.AddPPCCap,
                Order = (m) => ComponentOrder.PPC,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_PPC_C?PPC(?<eh>ER|Heavy)(?:_NU|_Sa)?_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}\r\n\t]\r\n}\r\n",
                AddToList = (x) => x.AddPPCCap,
                Order = (m) =>
                {
                    if (m.Groups["eh"].Value == "ER")
                        return ComponentOrder.ERPPC;
                    return ComponentOrder.HPPC;
                },
            },
            new StaticPatchPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_PPC_PPCSnub_(?:\\d+)-.+$"),
                Patch = "{\r\n\t\"MinRange\": 0,\r\n\t\"MaxRange\": 450,\r\n\t\"RangeSplit\": [\r\n\t\t270,\r\n\t\t390,\r\n\t\t450\r\n\t],\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"DistantVariance\": 0.5,\r\n\t\"DamageFalloffStartDistance\": 390,\r\n\t\"DamageFalloffEndDistance\": 450,\r\n\t\"DistantVarianceReversed\": false,\r\n\t\"RangedDmgFalloffType\": \"Linear\",\r\n\t\"isDamageVariation\": true,\r\n\t\"isStabilityVariation\": true,\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"SPPC_STD\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"Snub PPC fires normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"SPPC_FL\",\r\n\t\t\t\"UIName\": \"FL\",\r\n\t\t\t\"Name\": \"Focusing Lens\",\r\n\t\t\t\"Description\": \"The additional magnetic Focusing Lens allows to focus all particles into one projectile, concentrating the infliced damage to one location, at the cost of slighly increased heat generation.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"ShotsWhenFired\": -4,\r\n\t\t\t\"HeatGenerated\": 5,\r\n\t\t\t\"DamageMultiplier\": 5,\r\n\t\t\t\"InstabilityMultiplier\": 5,\r\n\t\t\t\"WeaponEffectID\": \"WeaponEffect-Weapon_PPC\"\r\n\t\t}\r\n\t]\r\n}",
                AddToList = (x) => x.AddPPCCapSnub,
                Order = (m) => ComponentOrder.SPPC,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Plasma_PlasmaRifle_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"FireTerrainChance\": 0.9,\r\n\t\"FireTerrainStrength\": 1,\r\n\t\"FireOnSuccessHit\": true\r\n}\r\n",
                Order = (m) => ComponentOrder.Plasma,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Flamer_C?Flamer_(?:\\d+|SPECIAL)-.+$"),
                ExtraData = ",\r\n\t\"FireTerrainChance\": 0.75,\r\n\t\"FireTerrainStrength\": 1,\r\n\t\"FireOnSuccessHit\": true\r\n}\r\n",
                Heat = true,
                Details = true,
                Order = (m) => ComponentOrder.Flamer,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_MachineGun_C?MachineGun(?<hl>Heavy|Light)?_(?:\\d+)-.+$"),
                Details = true,
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"MG_Full\",\r\n\t\t\t\"UIName\": \"x5\",\r\n\t\t\t\"Name\": \"Full Salvo\",\r\n\t\t\t\"Description\": \"Fires the MG at standard speed.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"MG_Double\",\r\n\t\t\t\"UIName\": \"x10\",\r\n\t\t\t\"Name\": \"Double Salvo\",\r\n\t\t\t\"Description\": \"Fires the MG at double speed, decreasing accuracy, increasing heat, but doubles the shots per turn.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"AccuracyModifier\": 4.0,\r\n\t\t\t\"ShotsWhenFired\": 5,\r\n\t\t\t\"HeatGenerated\": 5\r\n\t\t}\r\n\t],\r\n\t\"ShotsPerAmmo\": 0.2,\r\n\t\"VolleyDivisor\": 5\r\n}\r\n",
                Order = (m) =>
                {
                    if (m.Groups["hl"].Value == "Heavy")
                        return ComponentOrder.HMG;
                    if (m.Groups["hl"].Value == "Light")
                        return ComponentOrder.LMG;
                    return ComponentOrder.MG;
                },
            },
            new WeaponLRMPattern()
            {
                Check = new Regex("^Weapon_LRM_C?(?<n>N?)LRM(?<size>\\d+)_(?:\\d+)-.+$"),
                EnableArtemis = true,
                EnableNarc = true,
            },
            new WeaponLRMPattern()
            {
                Check = new Regex("^Weapon_ELRM_C?ELRM(?<size>\\d+)_(?:\\d+)-.+$"),
                EnableArtemis = false,
                EnableNarc = true,
                E = true,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Thunderbolt_Thunderbolt(?<size>\\d+)_(?:\\d+)-.+$"),
                Details = true,
                AddToList = (x) => x.AddNarcCompatible,
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": 1,\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Individual\",\r\n\t\"AMSHitChance\": 0.5,\r\n\t\"MissileHealth\": 5,\r\n\t\"ProjectileScale\": {\r\n\t\t\"x\": 2,\r\n\t\t\"y\": 2,\r\n\t\t\"z\": 2\r\n\t}\r\n}\r\n",
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "20")
                        return ComponentOrder.LRM20;
                    if (m.Groups["size"].Value == "15")
                        return ComponentOrder.LRM15;
                    if (m.Groups["size"].Value == "10")
                        return ComponentOrder.LRM10;
                    return ComponentOrder.LRM5;
                },
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
                Check = new Regex("^Weapon_SRM_C?SRM(?<size>\\d+)_(?:\\d+-.+|OneShot)$"),
                EnableArtemis = true,
                Streak = false,
                EnableNarc = true,
            },
            new WeaponSRMPattern()
            {
                Check = new Regex("^Weapon_SRM_C?SSRM(?<size>\\d+)_(?:\\d+-.+|OneShot)$"),
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
                Check = new Regex("^Weapon_(?:MRM|RL)_(?<mr>MRM|RL)(?<size>\\d+)_(?:\\d+-.+)$"),
            },
            new WeaponATMPattern()
            {
                Check = new Regex("^Weapon_ATM_ATM(?<size>\\d+)_(?:\\d+-.+)$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_TAG_(?:Standard|C3)(?:_\\d+)?-.+$"),
                Details = true,
                Boni = true,
                ExtraData = ",\r\n\t\"statusEffects\": [\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": true,\r\n\t\t\t\t\"showInStatusPanel\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"StatisticEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IncomingAttBonus\",\r\n\t\t\t\t\"Name\": \"TAG MARKED\",\r\n\t\t\t\t\"Details\": \"If targeted by non Clan LRMs/NLRMs this unit does not have an indirect fire modifier and all evasion is ignored. These effects do not stack with Artemis IV or a Narc Missile Beacon.\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_statusMarked\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"statisticData\": {\r\n\t\t\t\t\"appliesEachTick\": false,\r\n\t\t\t\t\"statName\": \"TAGCount\",\r\n\t\t\t\t\"operation\": \"Float_Add\",\r\n\t\t\t\t\"modValue\": \"1\",\r\n\t\t\t\t\"modType\": \"System.Single\"\r\n\t\t\t},\r\n\t\t\t\"tagData\": null,\r\n\t\t\t\"floatieData\": null,\r\n\t\t\t\"actorBurningData\": null,\r\n\t\t\t\"vfxData\": null,\r\n\t\t\t\"instantModData\": null,\r\n\t\t\t\"poorlyMaintainedEffectData\": null\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": false,\r\n\t\t\t\t\"showInStatusPanel\": false,\r\n\t\t\t\t\"hideApplicationFloatie\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"VFXEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IndicatorVFX\",\r\n\t\t\t\t\"Name\": \"Inferno VFX\",\r\n\t\t\t\t\"Details\": \"Visual indicator of the TAG effect\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_status_sensorsImpaired\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"vfxData\": {\r\n\t\t\t\t\"vfxName\": \"vfxPrfPrtl_TAGmarker_loop\",\r\n\t\t\t\t\"attachToImpactPoint\": true,\r\n\t\t\t\t\"location\": -1,\r\n\t\t\t\t\"isAttached\": true,\r\n\t\t\t\t\"facesAttacker\": false,\r\n\t\t\t\t\"isOneShot\": false,\r\n\t\t\t\t\"duration\": -1.0\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}",
                Order = (m) => ComponentOrder.TAG,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_TAG_(?:Light_Clan|Clan|C3)(?:_\\d+)?-.+$"),
                Details = true,
                Boni = true,
                ExtraData = ",\r\n\t\"statusEffects\": [\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": true,\r\n\t\t\t\t\"showInStatusPanel\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"StatisticEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IncomingAttBonus\",\r\n\t\t\t\t\"Name\": \"TAG MARKED\",\r\n\t\t\t\t\"Details\": \"If targeted by non Clan LRMs/NLRMs this unit does not have an indirect fire modifier and all evasion is ignored. These effects do not stack with Artemis IV or a Narc Missile Beacon.\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_statusMarked\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"statisticData\": {\r\n\t\t\t\t\"appliesEachTick\": false,\r\n\t\t\t\t\"statName\": \"TAGCountClan\",\r\n\t\t\t\t\"operation\": \"Float_Add\",\r\n\t\t\t\t\"modValue\": \"1\",\r\n\t\t\t\t\"modType\": \"System.Single\"\r\n\t\t\t},\r\n\t\t\t\"tagData\": null,\r\n\t\t\t\"floatieData\": null,\r\n\t\t\t\"actorBurningData\": null,\r\n\t\t\t\"vfxData\": null,\r\n\t\t\t\"instantModData\": null,\r\n\t\t\t\"poorlyMaintainedEffectData\": null\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": false,\r\n\t\t\t\t\"showInStatusPanel\": false,\r\n\t\t\t\t\"hideApplicationFloatie\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"VFXEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IndicatorVFX\",\r\n\t\t\t\t\"Name\": \"Inferno VFX\",\r\n\t\t\t\t\"Details\": \"Visual indicator of the TAG effect\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_status_sensorsImpaired\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"vfxData\": {\r\n\t\t\t\t\"vfxName\": \"vfxPrfPrtl_TAGmarker_loop\",\r\n\t\t\t\t\"attachToImpactPoint\": true,\r\n\t\t\t\t\"location\": -1,\r\n\t\t\t\t\"isAttached\": true,\r\n\t\t\t\t\"facesAttacker\": false,\r\n\t\t\t\t\"isOneShot\": false,\r\n\t\t\t\t\"duration\": -1.0\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}",
                Order = (m) => ComponentOrder.TAG,
            },
            new WeaponNarcPattern()
            {
                Check = new Regex("^Weapon_Narc_(?<sic>Standard|Improved|CNarc)_(?<bon>\\d+)-.+$"),
            },
            new OrderOnlyPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_MortarCAC_.+$"),
                Order = (m) => ComponentOrder.Artillery,
            },
            new OrderOnlyPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_BombBay$"),
                Order = (m) => ComponentOrder.BombBay,
            },
            new OrderOnlyPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_AMSCAC_.+$"),
                Order = (m) => ComponentOrder.AMS,
            },
            new IgnorePattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Laser_AI_Imaginary|Weapon_MeleeAttack|Weapon_DFAAttack|Weapon_Mortar_MechMortar|Weapon_Mortar_Thumper|Weapon_SniperArtillery|Weapon_LongTomArtillery$"),
            },
        };


        private static void GenerateAmmoBoxes(DataManager m, string targetfolder, IdCollector c)
        {
            string wepfolder = Path.Combine(targetfolder, "ammobox");
            //Directory.CreateDirectory(wepfolder);
            foreach (KeyValuePair<string, AmmunitionBoxDef> kv in m.AmmoBoxDefs)
            {
                if (kv.Key != kv.Value.Description.Id)
                    throw new InvalidDataException("id missmatch: " + kv.Key);
                bool found = false;
                foreach (Pattern<AmmunitionBoxDef> p in AmmoBoxPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, wepfolder, kv.Key, c);
                    found = true;
                    break;
                }
                if (!found)
                    FileLog.Log($"ammobox {kv.Key} does not have a pattern");
            }
        }

        private static readonly Pattern<AmmunitionBoxDef>[] AmmoBoxPatterns = new Pattern<AmmunitionBoxDef>[] {
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC20(?<t>_Half||AP|Precision|Tracer)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "AP")
                        return ComponentOrder.AmmoAC20AP;
                    if (m.Groups["t"].Value == "Precision")
                        return ComponentOrder.AmmoAC20P;
                    if (m.Groups["t"].Value == "Tracer")
                        return ComponentOrder.AmmoAC20Tr;
                    return ComponentOrder.AmmoAC20;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC10(?<t>_Half||AP|Precision|Tracer)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "AP")
                        return ComponentOrder.AmmoAC10AP;
                    if (m.Groups["t"].Value == "Precision")
                        return ComponentOrder.AmmoAC10P;
                    if (m.Groups["t"].Value == "Tracer")
                        return ComponentOrder.AmmoAC10Tr;
                    return ComponentOrder.AmmoAC10;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC5(?<t>_Half||AP|Precision|Tracer)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "AP")
                        return ComponentOrder.AmmoAC5AP;
                    if (m.Groups["t"].Value == "Precision")
                        return ComponentOrder.AmmoAC5P;
                    if (m.Groups["t"].Value == "Tracer")
                        return ComponentOrder.AmmoAC5Tr;
                    return ComponentOrder.AmmoAC5;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC2(?<t>_Half||AP|Precision|Tracer)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "AP")
                        return ComponentOrder.AmmoAC2AP;
                    if (m.Groups["t"].Value == "Precision")
                        return ComponentOrder.AmmoAC2P;
                    if (m.Groups["t"].Value == "Tracer")
                        return ComponentOrder.AmmoAC2Tr;
                    return ComponentOrder.AmmoAC2;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_LB(?<t>20|10|5|2)X(?:_Half)?$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "20")
                        return ComponentOrder.AmmoAC20LB;
                    if (m.Groups["t"].Value == "10")
                        return ComponentOrder.AmmoAC10LB;
                    if (m.Groups["t"].Value == "5")
                        return ComponentOrder.AmmoAC5LB;
                    if (m.Groups["t"].Value == "2")
                        return ComponentOrder.AmmoAC2LB;
                    return ComponentOrder.Invalid;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_(?<t>|H|L|SB|MAG)GAUSS$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "H")
                        return ComponentOrder.AmmoHGauss;
                    if (m.Groups["t"].Value == "L")
                        return ComponentOrder.AmmoLGauss;
                    if (m.Groups["t"].Value == "MAG")
                        return ComponentOrder.AmmoGaussMagshot;
                    if (m.Groups["t"].Value == "SB")
                        return ComponentOrder.AmmoGaussSB;
                    return ComponentOrder.AmmoGauss;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_(?<t>H|)MG(?:_Half)?"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "H")
                        return ComponentOrder.AmmoHMG;
                    return ComponentOrder.AmmoMG;
                },
            },

            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_(?:Arrow4|LongTom|Sniper|Thumper)$"),
                Order = (m) => ComponentOrder.AmmoArt,
            },

            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_ATM(?<t>|_ER|_HE)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_HE")
                        return ComponentOrder.AmmoATMHE;
                    if (m.Groups["t"].Value == "_ER")
                        return ComponentOrder.AmmoATMER;
                    return ComponentOrder.AmmoATM;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_ELRM$"),
                Order = (m) => ComponentOrder.AmmoELRM,
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_LRM(?<t>|_DF)(?:_Half)?$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_DF")
                        return ComponentOrder.AmmoLRMDF;
                    return ComponentOrder.AmmoLRM;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_MRM$"),
                Order = (m) => ComponentOrder.AmmoMRM,
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_iNarc(?<t>|_Explosive|_Haywire)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_Explosive")
                        return ComponentOrder.AmmoINARCEX;
                    if (m.Groups["t"].Value == "_Haywire")
                        return ComponentOrder.AmmoINARCH;
                    return ComponentOrder.AmmoINARC;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_Narc(?<t>|_Explosive)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_Explosive")
                        return ComponentOrder.AmmoNARCEX;
                    return ComponentOrder.AmmoNARC;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_SRM(?<t>_DF||_Inferno|Inferno)(?:_Half)?$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_DF")
                        return ComponentOrder.AmmoSRMDF;
                    if (m.Groups["t"].Value == "Inferno")
                        return ComponentOrder.AmmoSRMInf;
                    if (m.Groups["t"].Value == "_Inferno")
                        return ComponentOrder.AmmoSRMInf;
                    return ComponentOrder.AmmoSRM;
                },
            },
            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_Thunderbolt(?:20|15|10|5)$"),
                Order = (m) => ComponentOrder.AmmoLRM,
            },




            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_Plasma$"),
                Order = (m) => ComponentOrder.AmmoPlasma,
            },


            new OrderOnlyPattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_BombBay(?<t>_AP|_HE|_Inferno)$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_HE")
                        return ComponentOrder.BombHE;
                    if (m.Groups["t"].Value == "_AP")
                        return ComponentOrder.BombAP;
                    if (m.Groups["t"].Value == "_Inferno")
                        return ComponentOrder.BombI;
                    return ComponentOrder.Invalid;
                },
            },
            new IgnorePattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammunition_.*_ContentAmmunitionBoxDef$"),
            },
            new IgnorePattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^FuelTank_Extended|Ammo_AmmunitionBox_Generic_Flamer|AmmunitionBox_Flamer|Ammo_AmmunitionBox_Generic_S|Ammo_AmmunitionBox_Generic_LT$"),
            },
        };


        private static void GenerateJJComponents(DataManager m, string targetfolder, IdCollector c)
        {
            string wepfolder = Path.Combine(targetfolder, "jj");
            //Directory.CreateDirectory(wepfolder);
            foreach (KeyValuePair<string, JumpJetDef> kv in m.JumpJetDefs)
            {
                if (kv.Key != kv.Value.Description.Id)
                    throw new InvalidDataException("id missmatch: " + kv.Key);
                bool found = false;
                foreach (Pattern<MechComponentDef> p in OtherPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, wepfolder, kv.Key, c);
                    found = true;
                    break;
                }
                if (!found)
                    FileLog.Log($"jumpjet {kv.Key} does not have a pattern");
            }
        }
        private static void GenerateHSComponents(DataManager m, string targetfolder, IdCollector c)
        {
            string wepfolder = Path.Combine(targetfolder, "hs");
            //Directory.CreateDirectory(wepfolder);
            foreach (KeyValuePair<string, HeatSinkDef> kv in m.HeatSinkDefs)
            {
                if (kv.Key != kv.Value.Description.Id)
                    throw new InvalidDataException("id missmatch: " + kv.Key);
                bool found = false;
                foreach (Pattern<MechComponentDef> p in OtherPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, wepfolder, kv.Key, c);
                    found = true;
                    break;
                }
                if (!found)
                    FileLog.Log($"heatsink {kv.Key} does not have a pattern");
            }
        }
        private static void GenerateUpgradeComponents(DataManager m, string targetfolder, IdCollector c)
        {
            string wepfolder = Path.Combine(targetfolder, "upgrades");
            //Directory.CreateDirectory(wepfolder);
            foreach (KeyValuePair<string, UpgradeDef> kv in m.UpgradeDefs)
            {
                if (kv.Key != kv.Value.Description.Id)
                    throw new InvalidDataException("id missmatch: " + kv.Key);
                bool found = false;
                foreach (Pattern<MechComponentDef> p in OtherPatterns)
                {
                    Match match = p.Check.Match(kv.Key);
                    if (match == null || !match.Success)
                        continue;
                    p.Generate(kv.Value, match, wepfolder, kv.Key, c);
                    found = true;
                    break;
                }
                if (!found)
                    FileLog.Log($"upgrade {kv.Key} does not have a pattern");
            }
        }

        private static readonly Pattern<MechComponentDef>[] OtherPatterns = new Pattern<MechComponentDef>[] {
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Actuator_Axman_Hatchet|Gear_Actuator_Berserker_Hatchet|Gear_Actuator_BlackKnight_Sword|Gear_Actuator_Firestarter_Sword|Gear_Actuator_LifterClamp|Gear_Actuator_LiftHoist|Gear_Actuator_Prototype_Hatchet|Gear_Actuator_Vindicator_Sword$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Arm_Vestigal_Hand|Gear_BEX_.+|Gear_Quad_Actuators|Gear_Quirk_.+|Gear_RemoteSensorDispenser|Gear_UM-R50_Profile$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Actuator_.+$"),
                Order = (m) => ComponentOrder.Actuator,
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Addon_Artemis4$"),
                Order = (m) => ComponentOrder.Artemis,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Addon_Artillery_Loader_BS$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Addon_Artillery_Loader$"),
                Order = (m) => ComponentOrder.ArtilleryLoader,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Addon_PPCCapacitor$"),
                Order = (m) => ComponentOrder.PPCCap,
            },


            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_APod(?:_Clan)?$"),
                Order = (m) => ComponentOrder.APod,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_C3_.+$"),
                Order = (m) => ComponentOrder.C3,
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Cockpit_CommandConsole|Gear_Cockpit_CommandConsole_with_B2000|Gear_Cockpit_Corean_B-Tech|Gear_Cockpit_Tacticon_B2000_Battle_Computer$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Cockpit_.+$"),
                Order = (m) => ComponentOrder.CockpitMod,
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_EndoSteel_.+|Gear_FerroFibrous_.+$"),
                Order = (m) => ComponentOrder.Blocker,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_General_.+|Gear_Gyro_Cosara_Balanced_Gyros(?:_1)?$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Gyro_.+$"),
                Order = (m) => ComponentOrder.Gyro,
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Standard$"),
                Order = (m) => ComponentOrder.HSink,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Standard$"),
                Order = (m) => ComponentOrder.HSink,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Double|Gear_HeatSink_Clan_Double$"),
                Order = (m) => ComponentOrder.HSinkD,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_DHS_NAIS|Gear_HeatSink_FDHS|Gear_HeatSink_Generic_Compact$"),
                Order = (m) => ComponentOrder.HSinkX,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Bulk-Bank|Gear_HeatSink_Generic_Improved-Bank|Gear_HeatSink_Generic_Standard-Bank$"),
                Order = (m) => ComponentOrder.HBank,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Coolant_Pod$"),
                Order = (m) => ComponentOrder.HCPod,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Thermal-Exchanger-.+$"),
                Order = (m) => ComponentOrder.HEx,
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_JumpJet_LAMJet(?:_Idle)?|Gear_JumpJet_Vectored_Thrust_Kit|Gear_JumpJet_VTR_Vectored_Thrust|Gear_LAM_Systems$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_JumpJet_Generic_.+$"),
                Order = (m) => ComponentOrder.JJ,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_LAM_BombBay_Empty$"),
                Order = (m) => ComponentOrder.BombEmpty,
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Light_Engine|Gear_MASC_.*|Gear_SensorCAC_.+|Gear_Structure_Prototype_TSM|Gear_Triple_Strength_Myomer.*|Gear_TSM_Prototype_Bergan.*|Gear_VTOL|Gear_XL_Engine.*$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new IgnorePattern<MechComponentDef>()
            {
                Check = new Regex("Gear_Sensor_.+|Gear_EndoFerroCombo_.+|Gear_Mortar_.+|Gear_AMS_.+|Gear_Proto_EWS")
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_TAC.+_Clan$"),
                Order = (m) => ComponentOrder.TAC,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_TargetingTrackingSystem_.+$"),
                Order = (m) => ComponentOrder.TTS,
            },

            new IgnorePattern<MechComponentDef>()
            {
                Check = new Regex("repairkit_Weapon_.+|TargetDummyMod|TT_Gear_Sensor_.*")
            },
        };

        private class IdCollector
        {
            public readonly List<string> AddArtemisLRM = new List<string>();
            public readonly List<string> AddArtemisSRM = new List<string>();
            public readonly List<string> AddPPCCap = new List<string>();
            public readonly List<string> AddPPCCapSnub = new List<string>();
            public readonly List<string> AddNarcCompatible = new List<string>();
            public readonly Dictionary<string, ItemCollectionReplace> ICReplace = new Dictionary<string, ItemCollectionReplace>();
            public readonly Dictionary<string, WeaponAddonSplit> Splits = new Dictionary<string, WeaponAddonSplit>();
            public readonly Dictionary<ComponentOrder, List<string>> Order = new Dictionary<ComponentOrder, List<string>>();

            public void AddOrder(ComponentOrder order, string id)
            {
                if (!Order.TryGetValue(order, out List<string> l))
                {
                    l = new List<string>();
                    Order.Add(order, l);
                }
                l.Add(id);
            }
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
            public Func<Match, ComponentOrder> Order = null;
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                WriteTo(targetFolder, id, Patch);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
                if (Order != null)
                    c.AddOrder(Order(m), id);
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
                WriteTo(targetFolder, id, "{\r\n\t\"Custom\": {\r\n\t\t\"Flags\": [\r\n\t\t\t\"invalid\", \"hide\"\r\n\t\t]\r\n\t}\r\n}\r\n");
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

        private class OrderOnlyPattern<T> : Pattern<T>
        {
            public Func<Match, ComponentOrder> Order = null;
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                c.AddOrder(Order(m), id);
            }
        }
        private class IgnorePattern<T> : Pattern<T>
        {
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
            }
        }

        private class WeaponForwardingPattern : Pattern<WeaponDef>
        {
            public string ExtraData;
            public bool Details = false;
            public bool Heat = false;
            public bool Damage = true;
            public bool Boni = false;
            public Func<IdCollector, List<string>> AddToList = null;
            public Func<Match, ComponentOrder> Order = null;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = Forward(data, Details, Heat, Damage, Boni);
                p += ExtraData;
                WriteTo(targetFolder, id, p);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
                if (Order != null)
                    c.AddOrder(Order(m), id);
            }

            public static string Forward(WeaponDef data, bool details, bool heat, bool damage = true, bool boni = false, bool acc = true)
            {
                string p = $"{{\r\n\t\"MinRange\": {data.MinRange},\r\n\t\"MaxRange\": {data.MaxRange},\r\n\t\"RangeSplit\": [\r\n\t\t{data.RangeSplit[0]},\r\n\t\t{data.RangeSplit[1]},\r\n\t\t{data.RangeSplit[2]}\r\n\t],\r\n\t\"HeatGenerated\": {data.HeatGenerated}";
                if (damage)
                    p += $",\r\n\t\"Damage\": {data.Damage},\r\n\t\"Instability\": {data.Instability}";
                if (acc)
                    p += $",\r\n\t\"RefireModifier\": {data.RefireModifier},\r\n\t\"AccuracyModifier\": {data.AccuracyModifier}";
                if (heat)
                    p += $",\r\n\t\"HeatDamage\": {data.HeatDamage}";
                if (details)
                    p += $",\r\n\t\"Description\": {{\r\n\t\t\"Details\": {JsonConvert.ToString(data.Description.Details)}\r\n\t}}";
                if (boni)
                    p += $",\r\n\t\"BonusValueA\": \"{data.BonusValueA}\",\r\n\t\"BonusValueB\": \"{data.BonusValueB}\"";
                return p;
            }
        }
        private class WeaponACPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, false, false);
                p += ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,";
                string s = m.Groups["size"].Value;
                string li = m.Groups["li"].Value;
                p += $"\r\n\t\"RestrictedAmmo\": [\r\n\t\t\"Ammunition_LB{s}X\"\r\n\t]\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
                ComponentOrder o = ComponentOrder.Invalid;
                if (s == "20")
                    o = ComponentOrder.AC20;
                else if (s == "10")
                    o = ComponentOrder.AC10;
                else if (s == "5" && li == "L")
                    o = ComponentOrder.AC5Li;
                else if (s == "5")
                    o = ComponentOrder.AC5;
                else if (s == "2" && li == "L")
                    o = ComponentOrder.AC2Li;
                else if (s == "2")
                    o = ComponentOrder.AC2;
                c.AddOrder(o, id);
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
                string size = m.Groups["size"].Value;
                string ur = m.Groups["ur"].Value;
                p += $",\r\n\t\"PrefabIdentifier\": \"UAC{size}\",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,\r\n";
                p += $"\t\"RestrictedAmmo\": [\r\n\t\t\"Ammunition_AC{size}AP\",\r\n\t\t\"Ammunition_AC{size}Precision\",\r\n\t\t\"Ammunition_AC{size}Tracer\",\r\n\t\t\"Ammunition_LB{size}X\"\r\n\t],\r\n";
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
                ComponentOrder o = ComponentOrder.Invalid;
                if (size == "20")
                    o = ComponentOrder.AC20U;
                else if (size == "10")
                    o = ComponentOrder.AC10U;
                else if (size == "5" && ur == "R")
                    o = ComponentOrder.AC5R;
                else if (size == "5")
                    o = ComponentOrder.AC5U;
                else if (size == "2" && ur == "R")
                    o = ComponentOrder.AC2R;
                else if (size == "2")
                    o = ComponentOrder.AC2U;
                c.AddOrder(o, id);
            }
        }
        private class WeaponLBXPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int clustersize = int.Parse(m.Groups["size"].Value);
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
                ComponentOrder o = ComponentOrder.Invalid;
                if (clustersize == 20)
                    o = ComponentOrder.AC20LB;
                else if (clustersize == 10)
                    o = ComponentOrder.AC10LB;
                else if (clustersize == 5)
                    o = ComponentOrder.AC5LB;
                else if (clustersize == 2)
                    o = ComponentOrder.AC2LB;
                c.AddOrder(o, id);
            }
        }

        private class WeaponLRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis, EnableNarc, E=false;
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
                if (EnableNarc)
                    c.AddNarcCompatible.Add(id);
                string s = m.Groups["size"].Value;
                Capture n = m.Groups["n"];
                ComponentOrder o = ComponentOrder.Invalid;
                if (E)
                {
                    if (s == "20")
                        o = ComponentOrder.ELRM20;
                    else if (s == "15")
                        o = ComponentOrder.ELRM15;
                    else if (s == "10")
                        o = ComponentOrder.ELRM10;
                    else if (s == "5")
                        o = ComponentOrder.ELRM5;
                }
                else if (n != null && n.Value == "N")
                {
                    if (s == "20")
                        o = ComponentOrder.NLRM20;
                    else if (s == "15")
                        o = ComponentOrder.NLRM15;
                    else if (s == "10")
                        o = ComponentOrder.NLRM10;
                    else if (s == "5")
                        o = ComponentOrder.NLRM5;
                }
                else
                {
                    if (s == "20")
                        o = ComponentOrder.LRM20;
                    else if (s == "15")
                        o = ComponentOrder.LRM15;
                    else if (s == "10")
                        o = ComponentOrder.LRM10;
                    else if (s == "5")
                        o = ComponentOrder.LRM5;
                }
                c.AddOrder(o, id);
            }
        }
        private class WeaponSRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis, Streak, EnableNarc;
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
                if (EnableNarc)
                    c.AddNarcCompatible.Add(id);
                string s = m.Groups["size"].Value;
                ComponentOrder o = ComponentOrder.Invalid;
                if (Streak)
                {
                    if (s == "6")
                        o = ComponentOrder.SSRM6;
                    else if (s == "4")
                        o = ComponentOrder.SSRM4;
                    else if (s == "2")
                        o = ComponentOrder.SSRM2;
                }
                else
                {
                    if (s == "6")
                        o = ComponentOrder.SRM6;
                    else if (s == "4")
                        o = ComponentOrder.SRM4;
                    else if (s == "2")
                        o = ComponentOrder.SRM2;
                }
                c.AddOrder(o, id);
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
                string s = m.Groups["size"].Value;
                ComponentOrder o = ComponentOrder.Invalid;
                if (m.Groups["mr"].Value == "MRM")
                {
                    if (s == "40")
                        o = ComponentOrder.MRM40;
                    else if (s == "30")
                        o = ComponentOrder.MRM30;
                    else if (s == "20")
                        o = ComponentOrder.MRM20;
                    o = ComponentOrder.MRM10;
                }
                else
                {
                    if (s == "20")
                        o = ComponentOrder.RL20;
                    else if (s == "15")
                        o = ComponentOrder.RL15;
                    else if (s == "10")
                        o = ComponentOrder.RL10;
                }
                c.AddOrder(o, id);
            }
        }
        private class WeaponATMPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int size = data.ShotsWhenFired;
                string p = WeaponForwardingPattern.Forward(data, true, false, false);
                p += ",\r\n\t\"Damage\": 8,\r\n\t\"Instability\": 4,";
                p += $"\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1";
                p += ",\r\n\t\"ClusteringModifier\": 10,\r\n\t\"DirectFireModifier\": -4";
                string clu = "0.6333333";
                if (size == 3)
                    clu = "0.6666667";
                p += $",\r\n\t\"Custom\" : {{\r\n\t\t\"Clustering\": {{\r\n\t\t\t\"Base\": {clu},\r\n\t\t\t\"Steps\": [\r\n\t\t\t\t{{\r\n\t\t\t\t\t\"GunnerySkill\": 6,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}},\r\n\t\t\t\t{{\r\n\t\t\t\t\t\"GunnerySkill\": 10,\r\n\t\t\t\t\t\"Mod\": 0.03\r\n\t\t\t\t}}\r\n\t\t\t]\r\n\t\t}}\r\n\t}}";
                p += "\r\n}\r\n";
                WriteTo(targetFolder, id, p);
                string s = m.Groups["size"].Value;
                ComponentOrder o = ComponentOrder.Invalid;
                if (s == "12")
                    o = ComponentOrder.ATM12;
                else if (s == "9")
                    o = ComponentOrder.ATM9;
                else if (s == "6")
                    o = ComponentOrder.ATM6;
                else if (s == "3")
                    o = ComponentOrder.ATM3;
                c.AddOrder(o, id);
            }
        }
        private class WeaponNarcPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, true, false, false, false, false);
                float a = data.AccuracyModifier;
                p += $",\r\n\t\"AccuracyModifier\": {a}";
                if (int.TryParse(m.Groups["bon"].Value, out int r) && r > 0)
                {
                    a += r + 1;
                    p += $",\r\n\t\"BonusValueA\": \"+ {r + 1} Acc.\"";
                }
                else
                {
                    p += $",\r\n\t\"BonusValueA\": \"\"";
                }
                p += $",\r\n\t\"BonusValueB\": \"\"";
                p += "\r\n}";
                WriteTo(targetFolder, id, p);
                string s = m.Groups["sic"].Value;
                ComponentOrder o = ComponentOrder.Invalid;
                if (s == "Standard")
                    o = ComponentOrder.NARC;
                else if (s == "Improved")
                    o = ComponentOrder.INARC;
                else if (s == "CNarc")
                    o = ComponentOrder.NARC;
                c.AddOrder(o, id);
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
