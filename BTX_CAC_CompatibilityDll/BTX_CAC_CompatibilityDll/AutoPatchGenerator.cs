using BattleTech;
using BattleTech.Data;
using BTRandomMechComponentUpgrader;
using Extended_CE;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.RectTransform;

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
            GenerateULists(targetfolder, c);

            {
                File.WriteAllText(Path.Combine(targetfolder, "itemcollectionreplace.json"), JsonConvert.SerializeObject(c.ICReplace, Formatting.Indented));
                File.WriteAllText(Path.Combine(targetfolder, "addonsplit.json"), JsonConvert.SerializeObject(c.Splits, Formatting.Indented));
            }
        }

        private static void GenerateAdvMerges(DataManager m, string targetfolder, IdCollector c)
        {
            string amfolder = Path.Combine(targetfolder, "adv");
            Directory.CreateDirectory(amfolder);
            WriteAddComponentTagAdvMerge(amfolder, "artemis_lrm_attachable", c.AddArtemisLRM, "artemis_lrm_enable.json");
            WriteAddComponentTagAdvMerge(amfolder, "artemis_srm_attachable", c.AddArtemisSRM, "artemis_srm_enable.json");
            WriteAddComponentTagAdvMerge(amfolder, "ppc_capacitor_attachable", c.AddPPCCap, "ppc_cap_enable.json");
            WriteAddComponentTagAdvMerge(amfolder, "ppc_capacitor_attachable_snub", c.AddPPCCapSnub, "ppc_cap_snub_enable.json");
            WriteAddComponentTagAdvMerge(amfolder, "component_usesnarc", c.AddNarcCompatible, "narc_enable.json");
            WriteAddComponentTagAdvMerge(amfolder, "ballistic_tts_attachable", c.AddBallisticTTS, "ttsb_attachable.json");
            WriteAddComponentTagAdvMerge(amfolder, "energy_tts_attachable", c.AddEnergyTTS, "ttse_attachable.json");
            WriteAddComponentTagAdvMerge(amfolder, "missle_tts_attachable", c.AddMissleTTS, "ttsm_attachable.json");
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

        private static void WriteAddComponentTagAdvMerge(string amfolder, string tag, List<string> ids, string filename)
        {
            ids.Sort();
            string p = "{\r\n\t\"TargetIDs\": [\r\n\t\t";
            p += ids.Join((s) => $"\"{s}\"", ",\r\n\t\t");
            p += "\r\n\t],\r\n\t\"Instructions\": [\r\n\t\t{\r\n\t\t\t\"JSONPath\": \"$.ComponentTags.items\",\r\n\t\t\t\"Action\": \"ArrayAdd\",\r\n\t\t\t\"Value\": \"" + tag + "\"\r\n\t\t}\r\n\t]\r\n}\r\n";

            File.WriteAllText(Path.Combine(amfolder, filename), p);
        }

        private static readonly DateTime HCDate = DateTime.Parse("3050-01-01");
        private static readonly DateTime RCDate = DateTime.Parse("3065-01-01");
        enum ERating
        {
            F, D, C, B, A, CS, CSP
        }
        // [rarity][plus= -1,...,3, hc link, sldf link]
        private static readonly int[][] UListWeights = new int[][] {
                // F Awful
                new int[] {10, 100, 1, 0, 0, 0, 0},
                // D Poor
                new int[] {5, 100, 5, 1, 0, 0, 0},
                // C Average
                new int[] {1, 100, 10, 5, 1, 0, 0},
                // B Very Good
                new int[] {0, 100, 20, 10, 5, 1, 0},
                // A Exceptional
                new int[] {0, 50, 50, 25, 25, 5, 0},
                // CS ComStar
                new int[] {0, 1, 50, 100, 50, 10, 1},
                // CSP ComStar+
                new int[] {0, 0, 0, 0, 100, 1, 10},
            };
        private static readonly string[] FactionRoots = new string[]
        {
            "Davion", "Kurita", "Liao", "Marik", "Steiner", "Rasalhague", "Ives",
            "TaurianConcordat", "Outworld", "MagistracyOfCanopus",
            "Mercenaries",
        };
        private static void GenerateULists(string targetfolder, IdCollector c)
        {
            string ufolder = Path.Combine(targetfolder, "usublist");
            Directory.CreateDirectory(ufolder);
            string lfolder = Path.Combine(targetfolder, "ulist");
            Directory.CreateDirectory(lfolder);
            AddSublists(c);
            AddLinks(c);

            foreach (KeyValuePair<string, UpgradeSubList> kv in c.SubLists)
            {
                kv.Value.MainUpgradePath = FixWeights(kv.Value.MainUpgradePath);
                kv.Value.AmmoTypes = FixWeights(kv.Value.AmmoTypes);
                kv.Value.Addons = FixWeights(kv.Value.Addons);
                File.WriteAllText(Path.Combine(ufolder, $"{kv.Key}.json"), JsonConvert.SerializeObject(kv.Value, Formatting.Indented));
            }
            for (int i = (int)ERating.F; i <= (int)ERating.CSP; i++)
            {
                UpgradeList l = new UpgradeList
                {
                    AllowDowngrade = false,
                    CanRemove = new string[] { "Gear_HeatSink_Generic_Standard", "Gear_HeatSink_Clan_Double", "Gear_HeatSink_Generic_Double" },
                    Factions = fByER((ERating)i),
                    FactionPrefixWithNumber = Array.Empty<string>(),
                    LoadAdditions = c.SubLists.Keys.Where(x => x.StartsWith("Add")).OrderBy(x => x).ToArray(),
                    LoadUpgrades = c.SubLists.Keys.Where(x => !x.StartsWith("Add") && x != "HSC").OrderBy(x => x).ToArray(),
                    RemoveMaxFactor = 0.25f,
                    Sort = i,
                    UpgradePerComponentChance = 1,
                    WeightLookupTable = UListWeights[i],
                };
                File.WriteAllText(Path.Combine(lfolder, $"Generic_{(ERating)i}.json"), JsonConvert.SerializeObject(l, Formatting.Indented));
            }
            {
                UpgradeList l = new UpgradeList
                {
                    AllowDowngrade = false,
                    CanRemove = new string[] { "Gear_HeatSink_Generic_Standard", "Gear_HeatSink_Clan_Double", "Gear_HeatSink_Generic_Double" },
                    Factions = new string[] { "ClansB" },
                    FactionPrefixWithNumber = Array.Empty<string>(),
                    LoadAdditions = c.SubLists.Keys.Where(x => x.StartsWith("Add")).OrderBy(x => x).ToArray(),
                    LoadUpgrades = c.SubLists.Keys.Where(x => !x.StartsWith("Add") && x != "HS").OrderBy(x => x).ToArray(),
                    RemoveMaxFactor = 0.25f,
                    Sort = ((int)ERating.CSP) + 1,
                    UpgradePerComponentChance = 1,
                    WeightLookupTable = UListWeights[(int)ERating.B],
                };
                File.WriteAllText(Path.Combine(lfolder, $"Generic_ClanB.json"), JsonConvert.SerializeObject(l, Formatting.Indented));
            }

            string[] fByER(ERating i)
            {
                if (i == ERating.F)
                {
                    return FactionRoots.Select(x => $"{x}F").Concat(FactionRoots.Select(x => $"{x}Locals")).ToArray();
                }
                else if (i == ERating.CS)
                {
                    return new string[] { "ComStar" };
                }
                else if (i == ERating.CSP)
                {
                    return new string[] { "ComStarPlus" };
                }
                return FactionRoots.Select(x => $"{x}{i}").ToArray();
            }
        }

        private static void AddLinks(IdCollector c)
        {
            c.AddSubList("AC2", "Weapon_Autocannon_UAC2_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC2", "Weapon_Autocannon_UAC2_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("AC2", "Weapon_Autocannon_RAC2_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);
            c.AddSubList("AC2", "Weapon_Autocannon_LAC2_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);
            c.AddSubList("AC2", "Weapon_Autocannon_LB2X_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC2", "Weapon_Autocannon_LB2X_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);

            c.AddSubList("AC5", "Weapon_Autocannon_UAC5_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC5", "Weapon_Autocannon_UAC5_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("AC5", "Weapon_Autocannon_RAC5_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);
            c.AddSubList("AC5", "Weapon_Autocannon_LAC5_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);
            c.AddSubList("AC5", "Weapon_Autocannon_LB5X_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC5", "Weapon_Autocannon_LB5X_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);

            c.AddSubList("AC10", "Weapon_Autocannon_UAC10_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC10", "Weapon_Autocannon_UAC10_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("AC10", "Weapon_Autocannon_LB10X_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC10", "Weapon_Autocannon_LB10X_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);

            c.AddSubList("AC20", "Weapon_Autocannon_UAC20_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC20", "Weapon_Autocannon_UAC20_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("AC20", "Weapon_Autocannon_LB20X_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("AC20", "Weapon_Autocannon_LB20X_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);


            c.AddSubList("LargeLaser", "Weapon_Laser_BinaryLaserCannon_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, null, true);
            c.AddSubList("LargeLaser", "Weapon_Laser_LargeLaserER_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("LargeLaser", "Weapon_Laser_LargeLaserER_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("LargeLaser", "Weapon_Laser_LargeLaserPulse_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("LargeLaser", "Weapon_Laser_LargeLaserPulse_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("LargeLaser", "Weapon_Laser_LargeLaserXPulse_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);

            c.AddSubList("MediumLaser", "Weapon_Laser_MediumLaserER_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("MediumLaser", "Weapon_Laser_MediumLaserER_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("MediumLaser", "Weapon_Laser_MediumLaserPulse_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("MediumLaser", "Weapon_Laser_MediumLaserPulse_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("MediumLaser", "Weapon_Laser_MediumLaserXPulse_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);

            c.AddSubList("SmallLaser", "Weapon_Laser_SmallLaserER_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("SmallLaser", "Weapon_Laser_SmallLaserER_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("SmallLaser", "Weapon_Laser_SmallLaserPulse_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("SmallLaser", "Weapon_Laser_SmallLaserPulse_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("SmallLaser", "Weapon_Laser_SmallLaserXPulse_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);

            c.AddSubList("PPC", "Weapon_PPC_PPCER_0-STOCK", Array.Empty<UpgradeEntry>(), null, 5, null, true);
            c.AddSubList("PPC", "Weapon_PPC_PPCER_NU_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, HCDate, true);
            c.AddSubList("PPC", "Weapon_PPC_PPCSnub_0-STOCK", Array.Empty<UpgradeEntry>(), null, 4, RCDate, true);
        }

        private static void AddSublists(IdCollector c)
        {
            c.SubLists["Add_Gyro"] = new UpgradeSubList
            {
                MainUpgradePath = new UpgradeEntry[]
                            {
                    new UpgradeEntry
                    {
                        ID = "",
                        AllowDowngrade = true,
                        ListLink = false,
                        MinDate = DateTime.MinValue,
                        Weight = 0,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Gyro_Coventry_Mark-75",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Gyro_Friedhof_Kite",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Gyro_Rawlings_StabiliTrak-5",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                            },
                AmmoTypes = Array.Empty<UpgradeEntry>(),
                Addons = Array.Empty<UpgradeEntry>(),
            };
            c.SubLists["Add_Cockpit"] = new UpgradeSubList
            {
                MainUpgradePath = new UpgradeEntry[]
                {
                    new UpgradeEntry
                    {
                        ID = "",
                        AllowDowngrade = true,
                        ListLink = false,
                        MinDate = DateTime.MinValue,
                        Weight = 0,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Cockpit_Ceres_Metals_Braced",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Cockpit_Kallon_Industries_A6",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Cockpit_Majesty_M_M_60KL",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                    new UpgradeEntry
                    {
                        ID = "Gear_Cockpit_StarCorps_Improved",
                        AllowDowngrade = false,
                        ListLink = true,
                        MinDate = DateTime.MinValue,
                        Weight = 3,
                    },
                },
                AmmoTypes = Array.Empty<UpgradeEntry>(),
                Addons = Array.Empty<UpgradeEntry>(),
            };
        }

        private static UpgradeEntry[] FixWeights(UpgradeEntry[] u)
        {
            List<UpgradeEntry> l = u.ToList();
            l.Sort((a, b) => a.Weight == b.Weight ? a.ID.CompareTo(b.ID) : a.Weight - b.Weight);
            if (!l.Any(x => x.Weight == 3))
            {
                foreach (UpgradeEntry e in l)
                {
                    if (e.Weight == 2)
                        e.Weight = 3;
                }
            }
            foreach (UpgradeEntry e in l)
            {
                e.Weight++; // -1 weapons exists
            }
            return l.ToArray();
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
                Check = new Regex("^Weapon_Autocannon_(?<li>L?)AC(?<size>\\d+)_(?<plus>\\d+|SPECIAL)-.+$"),
            },
            new WeaponHGaussPattern()
            {
                Check = new Regex("^Weapon_Gauss_HeavyGauss(?<mag>_NU|_Sa|Magshot)?_(?<plus>\\d+)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Gauss_(?<hl>C|Light)?Gauss(?<mag>_NU|_Sa|Magshot)?_(?<plus>\\d+)-.+$"),
                ExtraData = "\r\n}\r\n",
                Details = true,
                Damage = false,
                Order = (m) =>
                {
                    if (m.Groups["mag"].Value == "Magshot")
                        return ComponentOrder.GaussMagshot;
                    if (m.Groups["hl"].Value == "Light")
                        return ComponentOrder.GaussLight;
                    return ComponentOrder.Gauss;
                },
                SubList = (d, id, m, c) =>
                {
                    string hl = m.Groups["hl"].Value;
                    string mag = m.Groups["mag"].Value;
                    string list;
                    if (mag == "Magshot")
                        list = "GaussMag";
                    else
                        list = $"Gauss{hl}";
                    if ((mag == "" || mag == "_Sa") && hl == "")
                        list = "SLDF" + list;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList(list, id, Array.Empty<string>(), new string[] { d.AmmoCategoryToAmmoBoxId }, lvl);
                    c.AddBallisticTTS.Add(id);
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Gauss_Silver_Bullet_Gauss_(?<plus>\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"FireDelayMultiplier\": 0,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"BallisticDamagePerPallet\": true,\r\n\t\"ShotsWhenFired\": 1,\r\n\t\"ProjectilesPerShot\": 12,\r\n\t\"Damage\": 108,\r\n\t\"Instability\": 60,\r\n\t\"ProjectileScale\": {\r\n\t\t\"x\": 0.2,\r\n\t\t\"y\": 0.2,\r\n\t\t\"z\": 0.2\r\n\t}\r\n}",
                Details = false,
                Damage = false,
                Order = (m) => ComponentOrder.GaussSB,
                SubList = (d, id, m, c) =>
                {
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList("GaussSB", id, Array.Empty<string>(), new string[] { d.AmmoCategoryToAmmoBoxId }, lvl);
                    c.AddBallisticTTS.Add(id);
                },
            },
            new WeaponUACPattern()
            {
                Check = new Regex("^Weapon_Autocannon_(?<ur>C?[UR])AC(?<size>\\d+)(?<sl>_NU|_Sa)?_(?<plus>\\d+)-.+$"),
            },
            new WeaponLBXPattern()
            {
                Check = new Regex("^Weapon_Autocannon_(?<c>C?)LB(?<size>\\d+)X(?<sl>_NU|_Sa)?_(?<plus>\\d+)-.+$"),
            },
            new WeaponLaserPattern()
            {
                Check = new Regex("^Weapon_Laser_(?<c>C?)(?<size>Large|Medium|Small|Micro)LaserPulse(?<sl>_NU|_Sa)?_(?<plus>\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1",
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
                SubList = (d, id, m, c) =>
                {
                    string sl = WeaponForwardingPattern.GetSLDFPrefix(m);
                    string si = m.Groups["size"].Value;
                    string cl = m.Groups["cl"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{sl}{cl}{si}PLaser", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponLaserPattern()
            {
                Check = new Regex("^Weapon_Laser_(?<size>Large|Medium|Small)LaserXPulse_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"ProjectilesPerShot\": 1",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.XPLLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.XPMLaser;
                    return ComponentOrder.XPSLaser;
                },
                SubList = (d, id, m, c) =>
                {
                    string si = m.Groups["size"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{si}XPLaser", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponLaserPattern()
            {
                Check = new Regex("^Weapon_Laser_(?<c>C?)(?<size>Large|Medium|Small|Micro)LaserER(?<sl>_NU|_Sa)?_(?<plus>\\d+)-.+$"),
                ExtraData = "",
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
                SubList = (d, id, m, c) =>
                {
                    string sl = WeaponForwardingPattern.GetSLDFPrefix(m);
                    string si = m.Groups["size"].Value;
                    string cl = m.Groups["c"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{sl}{cl}{si}ERLaser", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponLaserPattern()
            {
                Check = new Regex("^Weapon_Laser_(?<size>Large|Medium|Small)Laser_(?<plus>-?\\d+)-.+$"),
                ExtraData = "",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.LLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.MLaser;
                    return ComponentOrder.SLaser;
                },
                SubList = (d, id, m, c) =>
                {
                    string si = m.Groups["size"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{si}Laser", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponLaserPattern()
            {
                Check = new Regex("^Weapon_Laser_C(?<size>Large|Medium|Small)LaserHeavy_(?<plus>\\d+)-.+$"),
                ExtraData = "",
                Details = true,
                Order = (m) =>
                {
                    if (m.Groups["size"].Value == "Large")
                        return ComponentOrder.HLLaser;
                    if (m.Groups["size"].Value == "Medium")
                        return ComponentOrder.HMLaser;
                    return ComponentOrder.HSLaser;
                },
                SubList = (d, id, m, c) =>
                {
                    string si = m.Groups["size"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"C{si}LaserHeavy", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
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
                SubList = (d, id, m, c) => {
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponLaserPattern()
            {
                Check = new Regex("^Weapon_Laser_BinaryLaserCannon_(?<plus>\\d+)-.+$"),
                ExtraData = ",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"ProjectilesPerShot\": 2",
                Order = (m) => ComponentOrder.BLaser,
                SubList = (d, id, m, c) =>
                {
                    string si = m.Groups["size"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"BLaser", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_PPC_PPC_(?<plus>-?\\d+)-.+$"),
                ExtraData = ",\r\n\t\"UnsafeJamChance\": 0.1,\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_FI_OFF\",\r\n\t\t\t\"UIName\": \"FI OFF\",\r\n\t\t\t\"Name\": \"Field Inhibitor OFF\",\r\n\t\t\t\"Description\": \"Disabled Field Inhibitor removes minimum range, but at the chance to misfire.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"FlatJammingChance\": 0.25,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.025,\r\n\t\t\t\"MinRange\": -90.0,\r\n\t\t\t\"AccuracyModifier\": 1.0\r\n\t\t}\r\n\t]\r\n}",
                AddToList = (x) => x.AddPPCCap,
                Order = (m) => ComponentOrder.PPC,
                SubList = (d, id, m, c) =>
                {
                    string si = m.Groups["size"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"PPC", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
                Details = true,
                DetailsMap = (d) => d.Description.Details + "\nPPCs can switch off their Field Inhibitor, allowing the weapon to ignore minimum range penalties, but risking a potentially destructive misfire."
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_PPC_(?<c>C?)PPC(?<eh>ER|Heavy)(?<sl>_NU|_Sa)?_(?<plus>\\d+)-.+$"),
                ExtraData = ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"PPCMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"PPC operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}\r\n\t]\r\n}\r\n",
                AddToList = (x) => x.AddPPCCap,
                Order = (m) =>
                {
                    if (m.Groups["eh"].Value == "ER")
                        return ComponentOrder.ERPPC;
                    return ComponentOrder.HPPC;
                },
                SubList = (d, id, m, c) =>
                {
                    string eh = m.Groups["eh"].Value;
                    string sl = eh == "ER" ? WeaponForwardingPattern.GetSLDFPrefix(m) : "";
                    string cl = m.Groups["c"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{sl}{cl}PPC{eh}", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponSPPCPattern()
            {
                Check = new Regex("^Weapon_PPC_PPCSnub_(?<plus>\\d+)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Plasma_PlasmaRifle_(?:\\d+)-.+$"),
                ExtraData = ",\r\n\t\"FireTerrainChance\": 0.9,\r\n\t\"FireTerrainStrength\": 1,\r\n\t\"FireOnSuccessHit\": true\r\n}\r\n",
                Order = (m) => ComponentOrder.Plasma,
                SubList = (d, id, m, c) => {
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Flamer_(?<c>C?)Flamer_(?<plus>\\d+|SPECIAL)-.+$"),
                ExtraData = ",\r\n\t\"FireTerrainChance\": 0.75,\r\n\t\"FireTerrainStrength\": 1,\r\n\t\"FireOnSuccessHit\": true\r\n}\r\n",
                Heat = true,
                Details = true,
                Order = (m) => ComponentOrder.Flamer,
                SubList = (d, id, m, c) =>
                {
                    string cl = m.Groups["c"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{cl}Flamer", id, Array.Empty<string>(), new string[] { }, lvl);
                    c.AddEnergyTTS.Add(id);
                },
                DetailsMap = (d) => Regex.Replace(d.Description.Details, "cause the targeted unit to shut down from overheating", "cause the targeted unit to shut down from overheating. Usage may lead to dangerous forest fires, if used in the wrong conditions"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_MachineGun_(?<c>C?)MachineGun(?<hl>Heavy|Light)?_(?<plus>\\d+)-.+$"),
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
                SubList = (d, id, m, c) =>
                {
                    string cl = m.Groups["c"].Value;
                    string hl = m.Groups["hl"].Value;
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"{cl}MG{hl}", id, Array.Empty<string>(), new string[] { d.AmmoCategoryToAmmoBoxId }, lvl);
                    c.AddBallisticTTS.Add(id);
                },
            },
            new WeaponLRMPattern()
            {
                Check = new Regex("^Weapon_LRM_(?<c>C?)(?<n>N?)LRM(?<size>\\d+)_(?<plus>\\d+)-.+$"),
                EnableArtemis = true,
                EnableNarc = true,
            },
            new WeaponLRMPattern()
            {
                Check = new Regex("^Weapon_ELRM_ELRM(?<size>\\d+)_(?<plus>\\d+)-.+$"),
                EnableArtemis = false,
                EnableNarc = false,
                E = true,
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_Thunderbolt_Thunderbolt(?<size>\\d+)_(?<plus>\\d+)-.+$"),
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
                SubList = (d, id, m, c) =>
                {
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"Thunderbolt{m.Groups["size"].Value}", id, Array.Empty<string>(), new string[] { d.AmmoCategoryToAmmoBoxId }, lvl);
                    c.AddMissleTTS.Add(id);
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
                Check = new Regex("^Weapon_SRM_(?<c>C?)SRM(?<size>\\d+)_((?<plus>\\d+)-.+|OneShot)$"),
                EnableArtemis = true,
                Streak = false,
                EnableNarc = true,
            },
            new WeaponSRMPattern()
            {
                Check = new Regex("^Weapon_SRM_(?<c>C?)SSRM(?<size>\\d+)_((?<plus>\\d+)-.+|OneShot)$"),
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
                Id = new string[]{ "Ammo_AmmunitionBox_Generic_SRMInferno_Half", "Ammo_AmmunitionBox_Generic_SRMInferno" },
                Div = 2,
                Type = "AmmunitionBox",
            },
            new WeaponMRMPattern()
            {
                Check = new Regex("^Weapon_(?:MRM|RL)_(?<mr>MRM|RL)(?<size>\\d+)_(?<plus>\\d+)-.+$"),
            },
            new WeaponATMPattern()
            {
                Check = new Regex("^Weapon_ATM_ATM(?<size>\\d+)_(?<plus>\\d+)-.+$"),
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_TAG_(?<ty>Standard|C3)(?:_(?<plus>\\d+))?-.+$"),
                Details = true,
                Boni = true,
                ExtraData = ",\r\n\t\"statusEffects\": [\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": true,\r\n\t\t\t\t\"showInStatusPanel\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"StatisticEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IncomingAttBonus\",\r\n\t\t\t\t\"Name\": \"TAG MARKED\",\r\n\t\t\t\t\"Details\": \"If targeted by non Clan LRMs/NLRMs this unit does not have an indirect fire modifier and all evasion is ignored. These effects do not stack with Artemis IV or a Narc Missile Beacon.\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_statusMarked\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"statisticData\": {\r\n\t\t\t\t\"appliesEachTick\": false,\r\n\t\t\t\t\"statName\": \"TAGCount\",\r\n\t\t\t\t\"operation\": \"Float_Add\",\r\n\t\t\t\t\"modValue\": \"1\",\r\n\t\t\t\t\"modType\": \"System.Single\"\r\n\t\t\t},\r\n\t\t\t\"tagData\": null,\r\n\t\t\t\"floatieData\": null,\r\n\t\t\t\"actorBurningData\": null,\r\n\t\t\t\"vfxData\": null,\r\n\t\t\t\"instantModData\": null,\r\n\t\t\t\"poorlyMaintainedEffectData\": null\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": false,\r\n\t\t\t\t\"showInStatusPanel\": false,\r\n\t\t\t\t\"hideApplicationFloatie\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"VFXEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IndicatorVFX\",\r\n\t\t\t\t\"Name\": \"Inferno VFX\",\r\n\t\t\t\t\"Details\": \"Visual indicator of the TAG effect\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_status_sensorsImpaired\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"vfxData\": {\r\n\t\t\t\t\"vfxName\": \"vfxPrfPrtl_TAGmarker_loop\",\r\n\t\t\t\t\"attachToImpactPoint\": true,\r\n\t\t\t\t\"location\": -1,\r\n\t\t\t\t\"isAttached\": true,\r\n\t\t\t\t\"facesAttacker\": false,\r\n\t\t\t\t\"isOneShot\": false,\r\n\t\t\t\t\"duration\": -1.0\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}",
                Order = (m) => ComponentOrder.TAG,
                SubList = (d, id, m, c) =>
                {
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"TAG{m.Groups["ty"].Value}", id, Array.Empty<string>(), Array.Empty<string>(), lvl);
                    c.AddEnergyTTS.Add(id);
                },
            },
            new WeaponForwardingPattern()
            {
                Check = new Regex("^Weapon_TAG_(?<ty>Light_Clan|Clan)_(?<plus>\\d+)-.+$"),
                Details = true,
                Boni = true,
                ExtraData = ",\r\n\t\"statusEffects\": [\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": true,\r\n\t\t\t\t\"showInStatusPanel\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"StatisticEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IncomingAttBonus\",\r\n\t\t\t\t\"Name\": \"TAG MARKED\",\r\n\t\t\t\t\"Details\": \"If targeted by non Clan LRMs/NLRMs this unit does not have an indirect fire modifier and all evasion is ignored. These effects do not stack with Artemis IV or a Narc Missile Beacon.\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_statusMarked\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"statisticData\": {\r\n\t\t\t\t\"appliesEachTick\": false,\r\n\t\t\t\t\"statName\": \"TAGCountClan\",\r\n\t\t\t\t\"operation\": \"Float_Add\",\r\n\t\t\t\t\"modValue\": \"1\",\r\n\t\t\t\t\"modType\": \"System.Single\"\r\n\t\t\t},\r\n\t\t\t\"tagData\": null,\r\n\t\t\t\"floatieData\": null,\r\n\t\t\t\"actorBurningData\": null,\r\n\t\t\t\"vfxData\": null,\r\n\t\t\t\"instantModData\": null,\r\n\t\t\t\"poorlyMaintainedEffectData\": null\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"durationData\": {\r\n\t\t\t\t\"duration\": 1,\r\n\t\t\t\t\"ticksOnActivations\": false,\r\n\t\t\t\t\"useActivationsOfTarget\": true,\r\n\t\t\t\t\"ticksOnEndOfRound\": false,\r\n\t\t\t\t\"ticksOnMovements\": true,\r\n\t\t\t\t\"stackLimit\": 1,\r\n\t\t\t\t\"clearedWhenAttacked\": false\r\n\t\t\t},\r\n\t\t\t\"targetingData\": {\r\n\t\t\t\t\"effectTriggerType\": \"OnHit\",\r\n\t\t\t\t\"triggerLimit\": 0,\r\n\t\t\t\t\"extendDurationOnTrigger\": 0,\r\n\t\t\t\t\"specialRules\": \"NotSet\",\r\n\t\t\t\t\"effectTargetType\": \"NotSet\",\r\n\t\t\t\t\"range\": 0,\r\n\t\t\t\t\"forcePathRebuild\": false,\r\n\t\t\t\t\"forceVisRebuild\": false,\r\n\t\t\t\t\"showInTargetPreview\": false,\r\n\t\t\t\t\"showInStatusPanel\": false,\r\n\t\t\t\t\"hideApplicationFloatie\": true\r\n\t\t\t},\r\n\t\t\t\"effectType\": \"VFXEffect\",\r\n\t\t\t\"Description\": {\r\n\t\t\t\t\"Id\": \"StatusEffect-TAG-IndicatorVFX\",\r\n\t\t\t\t\"Name\": \"Inferno VFX\",\r\n\t\t\t\t\"Details\": \"Visual indicator of the TAG effect\",\r\n\t\t\t\t\"Icon\": \"uixSvgIcon_status_sensorsImpaired\"\r\n\t\t\t},\r\n\t\t\t\"nature\": \"Debuff\",\r\n\t\t\t\"vfxData\": {\r\n\t\t\t\t\"vfxName\": \"vfxPrfPrtl_TAGmarker_loop\",\r\n\t\t\t\t\"attachToImpactPoint\": true,\r\n\t\t\t\t\"location\": -1,\r\n\t\t\t\t\"isAttached\": true,\r\n\t\t\t\t\"facesAttacker\": false,\r\n\t\t\t\t\"isOneShot\": false,\r\n\t\t\t\t\"duration\": -1.0\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}",
                Order = (m) => ComponentOrder.TAG,
                SubList = (d, id, m, c) =>
                {
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"TAG{m.Groups["ty"].Value}", id, Array.Empty<string>(), Array.Empty<string>(), lvl);
                    c.AddEnergyTTS.Add(id);
                },
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
                Check = new Regex("^Weapon_AMSCAC_(?<plus>\\d+)$"),
                Order = (m) => ComponentOrder.AMS,
                SubList = (d, id, m, c) =>
                {
                    if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                        c.AddSubList($"AMS", id, Array.Empty<string>(), new string[] { d.AmmoCategoryToAmmoBoxId }, lvl);
                    c.AddBallisticTTS.Add(id);
                },
            },
            new OrderOnlyPattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_AMSCAC_Clan$"),
                Order = (m) => ComponentOrder.AMS,
                SubList = (d, id, m, c) =>
                {
                    //if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                    //    c.AddSubList($"CAMS", id, Array.Empty<string>(), Array.Empty<string>(), lvl);
                    c.AddBallisticTTS.Add(id);
                },
            },
            new IgnorePattern<WeaponDef>()
            {
                Check = new Regex("^Weapon_Laser_AI_Imaginary|Weapon_MeleeAttack|Weapon_DFAAttack|Weapon_Mortar_MechMortar|Weapon_Mortar_Thumper|Weapon_SniperArtillery|Weapon_LongTomArtillery$"),
            },
        };


        private static void GenerateAmmoBoxes(DataManager m, string targetfolder, IdCollector c)
        {
            string wepfolder = Path.Combine(targetfolder, "ammoboxnew");
            Directory.CreateDirectory(wepfolder);
            Directory.CreateDirectory(Path.Combine(targetfolder, "itemcollections"));
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
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC20(?<t>AP||Precision|Tracer)$"),
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
                Double = true,
                Triple = true,
                ItemColAmount = 4,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC10(?<t>AP||Precision|Tracer)$"),
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
                Double = true,
                Triple = true,
                ItemColAmount = 3,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC5(?<t>AP||Precision|Tracer)$"),
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
                Half = true,
                Double = true,
                ItemColAmount = 2,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_AC2(?<t>AP||Precision|Tracer)$"),
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
                Half = true,
                Double = true,
                ItemColAmount = 1,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_LB(?<t>5|2)X?$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "5")
                        return ComponentOrder.AmmoAC5LB;
                    if (m.Groups["t"].Value == "2")
                        return ComponentOrder.AmmoAC2LB;
                    return ComponentOrder.Invalid;
                },
                Half = true,
                Double = true,
                ItemColAmount = 2,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_LB(?<t>20|10)X?$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "20")
                        return ComponentOrder.AmmoAC20LB;
                    if (m.Groups["t"].Value == "10")
                        return ComponentOrder.AmmoAC10LB;
                    return ComponentOrder.Invalid;
                },
                Double = true,
                Triple = true,
                ItemColAmount = 3,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_(?<t>|H|L|SB)GAUSS$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "H")
                        return ComponentOrder.AmmoHGauss;
                    if (m.Groups["t"].Value == "L")
                        return ComponentOrder.AmmoLGauss;
                    if (m.Groups["t"].Value == "SB")
                        return ComponentOrder.AmmoGaussSB;
                    return ComponentOrder.AmmoGauss;
                },
                Double = true,
                Triple = true,
                ItemColAmount = 1,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_MAGGAUSS$"),
                Order = (m) => ComponentOrder.AmmoGaussMagshot,
                Half = true,
                ItemColAmount = 1,
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

            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_Thumper$"),
                Order = (m) => ComponentOrder.AmmoArt,
                Half = true,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_Sniper$"),
                Order = (m) => ComponentOrder.AmmoArt,
                Double = true,
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_(?:Arrow4|LongTom)$"),
                Order = (m) => ComponentOrder.AmmoArt,
                Double = true,
                Triple = true,
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
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_LRM(?<t>|_DF)?$"),
                Order = (m) =>
                {
                    if (m.Groups["t"].Value == "_DF")
                        return ComponentOrder.AmmoLRMDF;
                    return ComponentOrder.AmmoLRM;
                },
                Half = true,
                Double = true,
                Triple = true,
                ItemColAmount = 4,
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
            new IgnorePattern<AmmunitionBoxDef>()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_SRM_Inferno_Half$"),
            },
            new AmmoBoxGenPattern()
            {
                Check = new Regex("^Ammo_AmmunitionBox_Generic_SRM(?<t>_DF||Inferno)?$"),
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
                Half = true,
                Double = true,
                ItemColAmount = 3,
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
                Check = new Regex("^Gear_Cockpit_(?<ty>Ceres_Metals|Kallon_Industries|Majesty_M_M|StarCorps).+$"),
                Order = (m) => ComponentOrder.CockpitMod,
                SubList = (d, id, m, c) =>
                {
                    string ty = m.Groups["ty"].Value;
                    int lvl = d.Description.UIName.Count(x => x == '+');
                    c.AddSubList($"Cockpit{ty}", id, Array.Empty<string>(), Array.Empty<string>(), lvl);
                },
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_EndoSteel_.+|Gear_FerroFibrous_.+|Gear_EndoFerroCombo_.+$"),
                Order = (m) => ComponentOrder.Blocker,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_General_.+|Gear_Gyro_Cosara_Balanced_Gyros(?:_1)?$"),
                Order = (m) => ComponentOrder.Fixed,
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_Gyro_(?<ty>\\w+).+$"),
                Order = (m) => ComponentOrder.Gyro,
                SubList = (d, id, m, c) =>
                {
                    string ty = m.Groups["ty"].Value;
                    if (ty == "Hermes")
                        ty = "Rawlings";
                    int lvl = d.Description.UIName.Count(x => x == '+');
                    c.AddSubList($"Gyro{ty}", id, Array.Empty<string>(), Array.Empty<string>(), lvl);
                },
            },

            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Standard$"),
                Order = (m) => ComponentOrder.HSink,
                SubList = (d, id, m, c) =>
                {
                    c.AddSubList("HS", id, Array.Empty<string>(), Array.Empty<string>(), 0);
                    c.AddSubList("HSC", id, Array.Empty<string>(), Array.Empty<string>(), 0);
                },
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Generic_Double$"),
                Order = (m) => ComponentOrder.HSinkD,
                SubList = (d, id, m, c) =>
                {
                    c.AddSubList("HS", id, Array.Empty<string>(), Array.Empty<string>(), 4, HCDate);
                    c.AddSubList("HS", id, Array.Empty<string>(), Array.Empty<string>(), 5);
                },
            },
            new OrderOnlyPattern<MechComponentDef>()
            {
                Check = new Regex("^Gear_HeatSink_Clan_Double$"),
                Order = (m) => ComponentOrder.HSinkD,
                SubList = (d, id, m, c) =>
                {
                    c.AddSubList("HSC", id, Array.Empty<string>(), Array.Empty<string>(), 3);
                },
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
                Check = new Regex("Gear_Sensor_.+|Gear_Mortar_.+|Gear_AMS_.+|Gear_Proto_EWS")
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
            public readonly List<string> AddBallisticTTS = new List<string>();
            public readonly List<string> AddMissleTTS = new List<string>();
            public readonly List<string> AddEnergyTTS = new List<string>();
            public readonly Dictionary<string, ItemCollectionReplace> ICReplace = new Dictionary<string, ItemCollectionReplace>();
            public readonly Dictionary<string, WeaponAddonSplit> Splits = new Dictionary<string, WeaponAddonSplit>();
            public readonly Dictionary<ComponentOrder, List<string>> Order = new Dictionary<ComponentOrder, List<string>>();
            public readonly Dictionary<string, UpgradeSubList> SubLists = new Dictionary<string, UpgradeSubList>();

            public void AddOrder(ComponentOrder order, string id)
            {
                if (!Order.TryGetValue(order, out List<string> l))
                {
                    l = new List<string>();
                    Order.Add(order, l);
                }
                l.Add(id);
            }
            public void AddSubList(string listname, string id, UpgradeEntry[] addons, string[] ammo, int lvl, DateTime? md = null, bool ll = false)
            {
                UpgradeEntry e = new UpgradeEntry
                {
                    AllowDowngrade = lvl == 0,
                    ID = id,
                    ListLink = ll,
                    Weight = lvl,
                    MinDate = md ?? DateTime.MinValue,
                };
                if (SubLists.TryGetValue(listname, out UpgradeSubList l))
                {
                    l.MainUpgradePath = l.MainUpgradePath.AddToArray(e);
                }
                else
                {
                    SubLists.Add(listname, new UpgradeSubList
                    {
                        MainUpgradePath = new UpgradeEntry[] { e },
                        Addons = addons,
                        AmmoTypes = ammo.Select((x) => new UpgradeEntry { ID = x, ListLink = false, Weight = 0 }).ToArray(),
                    });
                }
            }
            public void AddSubList(string listname, string id, string[] addons, string[] ammo, int lvl, DateTime? md = null, bool ll = false)
            {
                AddSubList(listname, id, addons.Select((x) => new UpgradeEntry { ID = x, ListLink = false, Weight = 0 }).ToArray(), ammo, lvl, md, ll);
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
            public Action<T, string, Match, IdCollector> SubList = null;
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                WriteTo(targetFolder, id, Patch);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
                if (Order != null)
                    c.AddOrder(Order(m), id);
                SubList?.Invoke(data, id, m, c);
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
            public Action<T, string, Match, IdCollector> SubList = null;
            public override void Generate(T data, Match m, string targetFolder, string id, IdCollector c)
            {
                c.AddOrder(Order(m), id);
                SubList?.Invoke(data, id, m, c);
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
            public Action<WeaponDef, string, Match, IdCollector> SubList = null;
            public Func<WeaponDef, string> DetailsMap = null;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = Forward(data, Details, Heat, Damage, Boni, true, DetailsMap);
                p += ExtraData;
                WriteTo(targetFolder, id, p);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
                if (Order != null)
                    c.AddOrder(Order(m), id);
                SubList?.Invoke(data, id, m, c);
            }

            public static string Forward(WeaponDef data, bool details, bool heat, bool damage = true, bool boni = false, bool acc = true, Func<WeaponDef, string> detailsMap = null)
            {
                string p = $"{{\r\n\t\"MinRange\": {data.MinRange},\r\n\t\"MaxRange\": {data.MaxRange},\r\n\t\"RangeSplit\": [\r\n\t\t{data.RangeSplit[0]},\r\n\t\t{data.RangeSplit[1]},\r\n\t\t{data.RangeSplit[2]}\r\n\t],\r\n\t\"HeatGenerated\": {data.HeatGenerated}";
                if (damage)
                    p += $",\r\n\t\"Damage\": {data.Damage},\r\n\t\"Instability\": {data.Instability}";
                if (acc)
                    p += $",\r\n\t\"RefireModifier\": {data.RefireModifier},\r\n\t\"AccuracyModifier\": {data.AccuracyModifier}";
                if (heat)
                    p += $",\r\n\t\"HeatDamage\": {data.HeatDamage}";
                if (details)
                {
                    string d = null;
                    if (detailsMap != null)
                        d = detailsMap(data);
                    if (d == null)
                        d = data.Description.Details;
                    p += $",\r\n\t\"Description\": {{\r\n\t\t\"Details\": {EscapeDetails(d)}\r\n\t}}";
                }
                if (boni)
                    p += $",\r\n\t\"BonusValueA\": \"{data.BonusValueA}\",\r\n\t\"BonusValueB\": \"{data.BonusValueB}\"";
                return p;
            }

            public static string GetSLDFPrefix(Match m)
            {
                return m.Groups["sl"].Value == "_NU" || m.Groups["c"].Value == "C" ? "" : "SLDF";
            }

            public static string EscapeDetails(string d)
            {
                d = Regex.Replace(d, "\n\nRequired Hardpoint: <b><color=#([\\da-fA-F]+)>\\w+</color></b>", "");
                return JsonConvert.ToString(d);
            }

            public static string BuildClustering(string basefld, IEnumerable<int> steps, float stepin, CustomClustering.ClusterStepType stepty, string deadfirefld, string artfld, IEnumerable<(int, string)> uac)
            {
                float ba = Get(basefld);
                string r = "\t\t\"Clustering\": {\r\n";
                r += $"\t\t\t\"StepType\": \"{stepty}\",\r\n";
                if (steps != null)
                {
                    float step = stepin / 100.0f;
                    r += "\t\t\t\"Steps\": [\r\n";
                    bool f = true;
                    foreach (int i in steps)
                    {
                        if (!f)
                            r += ",\r\n";
                        f = false;
                        r += $"\t\t\t\t{{\r\n\t\t\t\t\t\"GunnerySkill\": {i},\r\n\t\t\t\t\t\"Mod\": {step}\r\n\t\t\t\t}}";
                    }
                    r += "\r\n\t\t\t],\r\n";
                }
                if (deadfirefld != null)
                {
                    r += $"\t\t\t\"DeadfireBase\": {Get(deadfirefld)},\r\n";
                }
                if (artfld != null)
                {
                    r += $"\t\t\t\"ArtemisBase\": {Get(artfld)},\r\n";
                }
                if (uac != null)
                {
                    r += "\t\t\t\"UAC\": [\r\n";
                    bool f = true;
                    foreach ((int l, string fi) in uac)
                    {
                        if (!f)
                            r += ",\r\n";
                        f = false;
                        r += $"\t\t\t\t{{\r\n\t\t\t\t\t\"Shots\": {l},\r\n\t\t\t\t\t\"Base\": {Get(fi)}\r\n\t\t\t\t}}";
                    }
                    r += "\r\n\t\t\t],\r\n";
                }
                r += $"\t\t\t\"Base\": {ba}\r\n\t\t}}\r\n";

                return r;

                float Get(string field)
                {
                    FieldInfo f = typeof(ModSettings).Assembly.GetType("Extended_CE.Functionality.TTWeaponRanges").GetField(field, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    return (float)f.GetValue(null);
                }
            }
        }
        private class WeaponACPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, true, false, true, false, true, Desc);
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
                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                    c.AddSubList($"{li}AC{s}", id, Array.Empty<string>(), new string[] { $"Ammo_AmmunitionBox_Generic_AC{s}", $"Ammo_AmmunitionBox_Generic_AC{s}AP", $"Ammo_AmmunitionBox_Generic_AC{s}Precision", $"Ammo_AmmunitionBox_Generic_AC{s}Tracer" }, lvl);
                c.AddBallisticTTS.Add(id);
            }
            private static string Desc(WeaponDef d)
            {
                return d.Description.Details + "\nCan fire Standard, AP, Precision and Tracer rounds.";
            }
        }


        private class WeaponHGaussPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                float sr = data.ShortRange;
                float maxr = data.MaxRange;
                string p = WeaponForwardingPattern.Forward(data, true, false, false, false, true, Desc);
                p += $",\r\n\t\"DistantVariance\": 0.4,\r\n\t\"DamageFalloffStartDistance\": {sr},\r\n\t\"DamageFalloffEndDistance\": {maxr},\r\n\t\"DistantVarianceReversed\": false,\r\n\t\"RangedDmgFalloffType\": \"Quadratic\",\r\n\t\"isDamageVariation\": true,\r\n\t\"isStabilityVariation\": true";
                p += "\r\n}";

                WriteTo(targetFolder, id, p);

                c.AddBallisticTTS.Add(id);
                c.AddOrder(ComponentOrder.GaussHeavy, id);

                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                    c.AddSubList($"GaussHeavy", id, Array.Empty<string>(), new string[] { data.AmmoCategoryToAmmoBoxId }, lvl);
            }

            private static string Desc(WeaponDef d)
            {
                return Regex.Replace(d.Description.Details, "Unlike other Gauss Rifles the Heavy Gauss Rifle does suffer a slight recoil effect from firing", "Unlike other Gauss Rifles the Heavy Gauss Rifle does suffer a slight recoil effect from firing and Damage falloff at long ranges");
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
                int uacrapidfireacc = (int)AEPStatic.GetCESettings().UACRapidFireAccuracyLoss;
                string size = m.Groups["size"].Value;
                string ur = m.Groups["ur"].Value;
                string wdesc = Desc(ur, data);
                string p = WeaponForwardingPattern.Forward(data, true, false, true, false, true, (_) => wdesc);
                p += $",\r\n\t\"PrefabIdentifier\": \"UAC{size}\",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,\r\n";
                p += $"\t\"RestrictedAmmo\": [\r\n\t\t\"Ammunition_LB{size}X\"\r\n\t],\r\n";
                p += "\t\"Custom\": {\r\n";
                p += WeaponForwardingPattern.BuildClustering("clusterMod", null, 0.0f, CustomClustering.ClusterStepType.UAC, null, null, new[]
                    {
                        (2, "clusterModTwo"),
                        (3, "clusterModThree"),
                        (4, "clusterModFour"),
                    });
                p += "\t},\r\n\t\"Modes\": [\n";
                for (int mi = 1; mi <= shotSelection; mi++)
                {
                    int acc = 0;
                    if (!AEPStatic.GetCESettings().TabletopWeapons)
                    {
                        if (mi > 3)
                            acc = uacrapidfireacc + 1;
                        else if (mi > 1)
                            acc = uacrapidfireacc;
                    }
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
                    string jam;
                    if (mi == 1)
                        jam = "\t\t\t\"FlatJammingChance\": 0.1,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.01";
                    else
                        jam = "\t\t\t\"FlatJammingChance\": 0.2,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.02";
                    p += $"\t\t{{\r\n\t\t\t\"Id\": \"UACMode_{mi}\",\r\n\t\t\t\"UIName\": \"x{mi}\",\r\n\t\t\t\"Name\": \"{name}\",\r\n\t\t\t\"Description\": \"{desc}\",\r\n\t\t\t\"isBaseMode\": {basemode.ToString().ToLower()},\r\n\t\t\t\"ShotsWhenFired\": {sfired},\r\n\t\t\t\"AccuracyModifier\": {acc},\r\n\t\t\t\"HeatGenerated\": {heatmod},\r\n{jam}\r\n\t\t}}";
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
                string sl = WeaponForwardingPattern.GetSLDFPrefix(m);
                if (ur == "R")
                    sl = "";
                if (ur == "CU")
                    sl = "";
                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                {
                    c.AddSubList($"{sl}{ur}AC{size}", id, Array.Empty<string>(), new string[] { $"Ammo_AmmunitionBox_Generic_AC{size}", $"Ammo_AmmunitionBox_Generic_AC{size}AP", $"Ammo_AmmunitionBox_Generic_AC{size}Precision", $"Ammo_AmmunitionBox_Generic_AC{size}Tracer" }, lvl);
                }
                c.AddBallisticTTS.Add(id);
            }
            private static string Desc(string ur, WeaponDef d)
            {
                if (ur == "R")
                {
                    string r = d.Description.Details.Replace("suffer from substantial recoil effects from firing and consume ammo based on firing mode.", "suffer from increasing accuracy loss and ammo consumption increasing with the rate of fire. RAC weapons are able to fire Standard, AP, Precision and Tracer rounds, although AP, Precision and Tracer rounds have a chance to Jam the weapon.");
                    return r;
                }
                else
                {
                    string r = d.Description.Details.Replace("can use both Rapid Fire or Single Fire modes using the middle mouse button to change modes. Rapid Fire modes suffer from accuracy loss and consume two ammo per attack. UAC weapons are unable to use special munition types.", "can use both Rapid Fire or Single Fire modes. Rapid Fire modes suffer from accuracy loss and consume two ammo per attack. UAC weapons are able to fire Standard, AP, Precision and Tracer rounds, although AP, Precision and Tracer rounds have a chance to Jam the weapon.");
                    r = r.Replace("suffer from substantial recoil effects from firing and consume two ammo per attack.", "can use both Rapid Fire or Single Fire modes. Rapid Fire modes suffer from accuracy loss and consume two ammo per attack. UAC weapons are able to fire Standard, AP, Precision and Tracer rounds, although AP, Precision and Tracer rounds have a chance to Jam the weapon.");
                    return r;
                }
            }
        }
        private class WeaponLBXPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int clustersize = int.Parse(m.Groups["size"].Value);
                if (clustersize != data.ShotsWhenFired)
                    throw new InvalidDataException("lbx size missmatch " + id);
                string p = WeaponForwardingPattern.Forward(data, true, false, true, false, true, Desc);
                p += $",\r\n\t\"PrefabIdentifier\": \"LBX{clustersize}\",\t\n\t\"ShotsWhenFired\": {data.ShotsWhenFired},\r\n\t\"VolleyDivisor\": 1,\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"FireDelayMultiplier\": 1,\r\n";
                if (AEPStatic.GetCESettings().TTWpnRangeLBXClusterModifier)
                {
                    p += "\t\"Custom\": {\r\n";
                    p += WeaponForwardingPattern.BuildClustering(clustersize <= 2 ? "clusterModTwo" : "clusterMod", null, 0.0f, CustomClustering.ClusterStepType.Json, null, null, null);
                    p += "\t},\r\n";
                }
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
                string sl = WeaponForwardingPattern.GetSLDFPrefix(m);
                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                {
                    c.AddSubList($"{sl}{m.Groups["c"].Value}LBX{clustersize}", id, Array.Empty<string>(), new string[] { $"Ammo_AmmunitionBox_Generic_AC{clustersize}", $"Ammo_AmmunitionBox_Generic_AC{clustersize}AP", $"Ammo_AmmunitionBox_Generic_AC{clustersize}Precision", $"Ammo_AmmunitionBox_Generic_AC{clustersize}Tracer", $"Ammo_AmmunitionBox_Generic_LB{clustersize}X" }, lvl);
                }
                c.AddBallisticTTS.Add(id);
            }
            private static string Desc(WeaponDef d)
            {
                string r = d.Description.Details.Replace("can use both Cluster or Solid Slug Fire modes using the middle mouse button to change modes. Cluster munitions are required for Cluster Fire mode, standard Autocannon munitions for Solid Slug. LB-X weapons are unable to use other special munitions.", "can switch between Standard, AP, Precision, Tracer and LBX Cluster rounds.");
                r = r.Replace("fires cluster munitions and suffers recoil from continuous fire.", "can switch between Standard, AP, Precision, Tracer and LBX Cluster rounds.");
                return r;
            }
        }

        private class WeaponLaserPattern : Pattern<WeaponDef>
        {
            public string ExtraData;
            public bool Details = false;
            public bool Heat = false;
            public bool Damage = true;
            public bool Boni = false;
            public Func<IdCollector, List<string>> AddToList = null;
            public Func<Match, ComponentOrder> Order = null;
            public Action<WeaponDef, string, Match, IdCollector> SubList = null;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, Details, Heat, Damage, Boni);
                p += ExtraData;
                p += ",\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"LMode_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"Laser operates normally.\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t},";
                int torem = Math.Min((int)data.Damage / 5 / 2, data.HeatGenerated / 3 / 2);
                p += $"\r\n\t\t{{\r\n\t\t\t\"Id\": \"LMode_LowPower\",\r\n\t\t\t\"UIName\": \"LP\",\r\n\t\t\t\"Name\": \"Low Power Mode\",\r\n\t\t\t\"Description\": \"Using less Energy to create the Laser reduces heat generation at the cost of Damage.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamagePerShot\": -{torem * 5},\r\n\t\t\t\"HeatGenerated\": -{torem * 3}\r\n\t\t}}\r\n\t]\r\n}}\r\n";
                WriteTo(targetFolder, id, p);
                if (AddToList != null)
                    AddToList(c)?.Add(id);
                if (Order != null)
                    c.AddOrder(Order(m), id);
                SubList?.Invoke(data, id, m, c);
            }
        }

        private class WeaponSPPCPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                float minr = 0 * 30.0f;
                float sr = 9 * 30f;
                float mr = 13 * 30f;
                float maxr = 15 * 30f;
                string p = $"{{\r\n\t\"MinRange\": {minr},\r\n\t\"MaxRange\": {maxr},\r\n\t\"RangeSplit\": [\r\n\t\t{sr},\r\n\t\t{mr},\r\n\t\t{maxr}\r\n\t]";
                p += $",\r\n\t\"Description\": {{\r\n\t\t\"Details\": {WeaponForwardingPattern.EscapeDetails("A unique modification of a standard Particle Projector Cannon, the Snub PPC shortens a standard PPC, leading to a lighter and more compact weapon system. The trade-off is a shorter range and damage fallof at long ranges.\nThe last remaining (magnetic) focusing lens can be disabled, leading to an increase in damage, but spread over a larger area.")}\r\n\t}}";
                p += ",\r\n\t\"ImprovedBallistic\": false,\r\n\t\"BallisticDamagePerPallet\": false,\r\n\t\"HasShells\": false,\r\n\t\"DisableClustering\": true,\r\n\t\"HitGenerator\": \"Cluster\"";
                p += $",\r\n\t\"DistantVariance\": 0.5,\r\n\t\"DamageFalloffStartDistance\": {sr},\r\n\t\"DamageFalloffEndDistance\": {maxr},\r\n\t\"DistantVarianceReversed\": false,\r\n\t\"RangedDmgFalloffType\": \"Quadratic\",\r\n\t\"isDamageVariation\": true,\r\n\t\"isStabilityVariation\": true";
                p += ",\r\n\t\"Modes\": [";

                float fl_targetdmg = (data.Damage - 15) * 5 + 50;
                float fl_targetstab = (data.Instability - 5) * 5 + 20;
                p += $"\r\n\t\t{{\r\n\t\t\t\"Id\": \"SPPC_STD\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"Snub PPC fires normally.\",\r\n\t\t\t\"isBaseMode\": true,\r\n\t\t\t\"ShotsWhenFired\": -4,\r\n\t\t\t\"DamagePerShot\": {fl_targetdmg - data.Damage},\r\n\t\t\t\"Instability\": {fl_targetstab - data.Instability},\r\n\t\t\t\"WeaponEffectID\": \"WeaponEffect-Weapon_PPC\"\r\n\t\t}}";
                p += ",\r\n\t\t{\r\n\t\t\t\"Id\": \"SPPC_FLO\",\r\n\t\t\t\"UIName\": \"FLO\",\r\n\t\t\t\"Name\": \"No Focusing Lens\",\r\n\t\t\t\"Description\": \"Disabled focusing lens leads to more overall damage, spread all over the target.\",\r\n\t\t\t\"isBaseMode\": false\r\n\t\t}";
                p += "\r\n\t]\r\n}";

                WriteTo(targetFolder, id, p);

                c.AddPPCCapSnub.Add(id);
                c.AddEnergyTTS.Add(id);
                c.AddOrder(ComponentOrder.SPPC, id);

                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                    c.AddSubList($"SnubPPC", id, Array.Empty<string>(), new string[] { }, lvl);
            }
        }

        private class WeaponLRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis, EnableNarc, E = false;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                float minr = data.MinRange;
                int size = data.ShotsWhenFired;
                string p = WeaponForwardingPattern.Forward(data, true, false, true, false, true, Desc);
                p += $",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1,\r\n\t\"UnsafeJamChance\": 0.1";
                p += ",\r\n\t\"Custom\": {\r\n";
                p += WeaponForwardingPattern.BuildClustering("clusterMod", null, 0.0f, CustomClustering.ClusterStepType.LRM, "clusterModDeadFire", "clusterMod_PlusTwo", null);
                p += "\t},\r\n\t\"Modes\": [\r\n\t\t{\r\n\t\t\t\"Id\": \"LRM_Std\",\r\n\t\t\t\"UIName\": \"STD\",\r\n\t\t\t\"Name\": \"Standard\",\r\n\t\t\t\"Description\": \"\",\r\n\t\t\t\"isBaseMode\": true\r\n\t\t}";
                if (minr > 0)
                {
                    p += $",\r\n\t\t{{\r\n\t\t\t\"Id\": \"LRM_HotLoad\",\r\n\t\t\t\"UIName\": \"HL\",\r\n\t\t\t\"Name\": \"Hot Load\",\r\n\t\t\t\"Description\": \"Missiles are armed inside the launcher instead of after launching, removing minimum range but having a chance to jam.\",\r\n\t\t\t\"isBaseMode\": false,\r\n\t\t\t\"DamageOnJamming\": true,\r\n\t\t\t\"AccuracyModifier\": 1.0,\r\n\t\t\t\"FlatJammingChance\": 0.25,\r\n\t\t\t\"GunneryJammingBase\": 10,\r\n\t\t\t\"GunneryJammingMult\": 0.025,\r\n\t\t\t\"MinRange\": {-minr}\r\n\t\t}}";
                }
                p += "\r\n\t]\r\n}\r\n";
                WriteTo(targetFolder, id, p);
                if (EnableArtemis)
                    c.AddArtemisLRM.Add(id);
                if (EnableNarc)
                    c.AddNarcCompatible.Add(id);
                string s = m.Groups["size"].Value;
                string n = m.Groups["n"].Value;
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
                else if (n == "N")
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
                c.AddMissleTTS.Add(id);
                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                {
                    UpgradeEntry[] ad;
                    if (EnableArtemis)
                        ad = new UpgradeEntry[] {
                                new UpgradeEntry
                                {
                                    ID = "",
                                    AllowDowngrade = false,
                                    ListLink = false,
                                    MinDate = DateTime.MinValue,
                                    Weight = 0,
                                },
                                new UpgradeEntry
                                {
                                    ID = "Gear_Addon_Artemis4",
                                    AllowDowngrade = false,
                                    ListLink = false,
                                    MinDate = HCDate,
                                    Weight = 4,
                                },
                                new UpgradeEntry
                                {
                                    ID = "Gear_Addon_Artemis4",
                                    AllowDowngrade = false,
                                    ListLink = false,
                                    MinDate = DateTime.MinValue,
                                    Weight = 5,
                                },
                            };
                    else
                        ad = Array.Empty<UpgradeEntry>();
                    c.AddSubList($"{m.Groups["c"].Value}{n}{(E ? "E" : "")}LRM{size}", id, ad, new string[] { $"Ammo_AmmunitionBox_Generic_LRM", $"Ammo_AmmunitionBox_Generic_LRM_DF" }, lvl);
                }
            }

            private string Desc(WeaponDef d)
            {
                string r = d.Description.Details;
                if (E)
                {
                    r = Regex.Replace(r, "Extended LRMs missile modifier is twice as detrimental when within minimum range and cannot benefit from a Narc Missile Beacon", "Extended LRMs use an additional booster rocket to increase the weapons range, at the cost of decreased accuracy while in minimum range.\nMissile weapons have a negative accuracy modifier that is percentage based and applied after the standard To Hit chance is calculated");
                }
                else
                {
                    string re;
                    if (EnableArtemis && EnableNarc)
                        re = "LRM weapons accuracy modifier can be positively affected by an attached Artemis IV FCS or Narc Homing Pod on the target (not both)";
                    else if (EnableArtemis)
                        re = "LRM weapons accuracy modifier can be positively affected by an attached Artemis IV FCS";
                    else if (EnableNarc)
                        re = "LRM weapons accuracy modifier can be positively affected by a Narc Homing Pod on the target";
                    else
                        re = "";
                    r = Regex.Replace(r, "LRM weapons accuracy modifier can be positively affected by Artemis IV, Narc Missile Beacon, or negatively affected by Anti Missile Systems", re);
                }
                if (d.MinRange > 0)
                    r += "\nBy arming the missiles in the launch tubes, minimum range penalties can be ignored, but risking a potentially destructive misfire.";
                return r;
            }
        }
        private class WeaponSRMPattern : Pattern<WeaponDef>
        {
            public bool EnableArtemis, Streak, EnableNarc;
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int size = data.ShotsWhenFired;
                string p = WeaponForwardingPattern.Forward(data, true, false, true, false, true, Desc);
                p += $",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"{(Streak ? "Streak" : "Individual")}\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1,\r\n";
                if (Streak)
                {
                    p += "\t\"RestrictedAmmo\": [\r\n\t\t\"Ammunition_SRMInferno\",\r\n\t\t\"Ammunition_SRM_DF\"\r\n\t],\r\n";
                }
                else
                {
                    p += "\t\"Custom\": {\r\n";
                    if (data.ShotsWhenFired >= 6)
                    {
                        p += WeaponForwardingPattern.BuildClustering("clusterMod", null, 0.0f, CustomClustering.ClusterStepType.SRM, "clusterModDeadFire", "clusterMod_PlusTwo", null);
                    }
                    else if (data.ShotsWhenFired >= 4)
                    {
                        p += WeaponForwardingPattern.BuildClustering("clusterModFour", null, 0.0f, CustomClustering.ClusterStepType.SRM, "clusterModFourDeadFire", "clusterModThree_PlusTwo", null);
                    }
                    else
                    {
                        p += WeaponForwardingPattern.BuildClustering("clusterModTwo", null, 0.0f, CustomClustering.ClusterStepType.SRM, "clusterModTwoDeadFire", "clusterModTwo_PlusTwo", null);
                    }

                    p += "\t},\r\n";
                }
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
                c.AddMissleTTS.Add(id);
                if (int.TryParse(m.Groups["plus"].Value, out int lvl) && id != "Weapon_SRM_SRM4_0-INV")
                {
                    UpgradeEntry[] ad;
                    if (EnableArtemis)
                    {
                        ad = new UpgradeEntry[] {
                            new UpgradeEntry
                            {
                                ID = "",
                                AllowDowngrade = false,
                                ListLink = false,
                                MinDate = DateTime.MinValue,
                                Weight = 0,
                            },
                            new UpgradeEntry
                            {
                                ID = "Gear_Addon_Artemis4",
                                AllowDowngrade = false,
                                ListLink = false,
                                MinDate = HCDate,
                                Weight = 4,
                            },
                            new UpgradeEntry
                            {
                                ID = "Gear_Addon_Artemis4",
                                AllowDowngrade = false,
                                ListLink = false,
                                MinDate = DateTime.MinValue,
                                Weight = 5,
                            },
                        };
                    }
                    else
                    {
                        ad = Array.Empty<UpgradeEntry>();
                    }
                    string[] abox;
                    if (Streak)
                    {
                        abox = new string[] { "Ammo_AmmunitionBox_Generic_SRM", "Ammo_AmmunitionBox_Generic_SRMInferno", "Ammo_AmmunitionBox_Generic_SRM_DF" };
                    }
                    else
                    {
                        abox = new string[] { "Ammo_AmmunitionBox_Generic_SRM" };
                    }
                    c.AddSubList($"{m.Groups["c"].Value}{(Streak ? "S" : "")}SRM{s}", id, ad, abox, lvl);
                }
            }

            private string Desc(WeaponDef d)
            {
                string r = d.Description.Details;
                if (Streak)
                {
                    r = Regex.Replace(r, "Unlike other missile systems, Streak based weaponry does not suffer from a Missile accuracy modifier, unless the target is using an active Anti Missile System", "Unlike other missile systems, Streak based weaponry does not suffer from a Missile accuracy modifier");
                }
                else
                {
                    string re;
                    if (EnableArtemis && EnableNarc)
                        re = "SRM weapons accuracy modifier can be positively affected by an attached Artemis IV FCS or Narc Homing Pod on the target (not both)";
                    else if (EnableArtemis)
                        re = "SRM weapons accuracy modifier can be positively affected by an attached Artemis IV FCS";
                    else if (EnableNarc)
                        re = "SRM weapons accuracy modifier can be positively affected by a Narc Homing Pod on the target";
                    else
                        re = "";
                    r = Regex.Replace(r, "SRM weapons accuracy modifier can be positively affected by Artemis IV, Narc Missile Beacon, or negatively affected by Anti Missile Systems", re);
                }
                return r;
            }
        }
        private class WeaponMRMPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                string p = WeaponForwardingPattern.Forward(data, true, false, true, false, true, Desc);
                int size = data.ShotsWhenFired;
                p += $",\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Individual\",\r\n\t\"AMSHitChance\": 0.5,\r\n\t\"MissileHealth\": 2,\r\n\t\"Unguided\": true";
                p += ",\r\n\t\"Custom\": {\r\n";
                p += WeaponForwardingPattern.BuildClustering("clusterModDeadFire", null, 0.0f, CustomClustering.ClusterStepType.LRM, null, null, null);
                p += "\r\n\t}\r\n}\r\n";
                WriteTo(targetFolder, id, p);
                string s = m.Groups["size"].Value;
                ComponentOrder o = ComponentOrder.Invalid;
                string mr = m.Groups["mr"].Value;
                if (mr == "MRM")
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
                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                    c.AddSubList($"{mr}{s}", id, Array.Empty<string>(), mr == "MRM" ? new string[] { data.AmmoCategoryToAmmoBoxId } : Array.Empty<string>(), lvl);
                c.AddMissleTTS.Add(id);
            }

            private string Desc(WeaponDef s)
            {
                return s.Description.Details.Replace("MRM weapons accuracy modifier is not affected by a Narc Missile Beacon, however will be negatively affected by Anti Missile Systems", "MRM weapons accuracy modifier is not affected by a Narc Homing Pod on the target").
                    Replace("Rocket Launcher weapons accuracy modifier is not affected by a Narc Missile Beacon, however will be negatively affected by Anti Missile Systems.", "Rocket Launcher weapons accuracy modifier is not affected by a Narc Missile Beacon.");
            }
        }
        private class WeaponATMPattern : Pattern<WeaponDef>
        {
            public override void Generate(WeaponDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                int size = data.ShotsWhenFired;
                string p = WeaponForwardingPattern.Forward(data, true, false, false, false, true, Desc);
                p += ",\r\n\t\"Damage\": 8,\r\n\t\"Instability\": 4,";
                p += $"\r\n\t\"ImprovedBallistic\": true,\r\n\t\"MissileVolleySize\": {size},\r\n\t\"MissileFiringIntervalMultiplier\": 1,\r\n\t\"MissileVolleyIntervalMultiplier\": 1,\r\n\t\"FireDelayMultiplier\": 1,\r\n\t\"HitGenerator\": \"Cluster\",\r\n\t\"AMSHitChance\": 0.0,\r\n\t\"MissileHealth\": 1";
                p += ",\r\n\t\"ClusteringModifier\": 10,\r\n\t\"DirectFireModifier\": -4";
                p += ",\r\n\t\"Custom\": {\r\n";
                p += WeaponForwardingPattern.BuildClustering(size == 3 ? "clusterModThree" : "clusterMod", null, 0.0f, CustomClustering.ClusterStepType.LRM, null, null, null );
                p += "\r\n\t}\r\n}\r\n";
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
                if (int.TryParse(m.Groups["plus"].Value, out int lvl))
                    c.AddSubList($"ATM{s}", id, Array.Empty<string>(), new string[] { "Ammo_AmmunitionBox_Generic_ATM", "Ammo_AmmunitionBox_Generic_ATM_ER", "Ammo_AmmunitionBox_Generic_ATM_HE" }, lvl);
            }

            private string Desc(WeaponDef s)
            {
                return s.Description.Details.Replace("ATM Missiles are able to switch between 3 different missile types during combat using the middle mouse button, each with their own range and damage profile", "ATM Launchers are able to switch between 3 different missile types during combat, each with their own range and damage profile").
                    Replace("ATM weapons accuracy modifier is positively affected by an integrated Artemis IV, and negatively affected by a target's Anti Missile Systems. ATM systems cannot benefit from a Narc Missile Beacon.", "ATM weapons accuracy modifier is positively affected by an integrated Artemis IV. ATM systems cannot benefit from a Narc Missile Beacon.");
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
                if (int.TryParse(m.Groups["bon"].Value, out int lvl))
                {
                    string[] am;
                    if (s == "Improved")
                        am = new string[] { "Ammo_AmmunitionBox_Generic_iNarc", "Ammo_AmmunitionBox_Generic_iNarc_Explosive", "Ammo_AmmunitionBox_Generic_iNarc_Haywire" };
                    else
                        am = new string[] { "Ammo_AmmunitionBox_Generic_Narc", "Ammo_AmmunitionBox_Generic_Narc_Explosive" };
                    c.AddSubList($"NARC{s}", id, Array.Empty<string>(), am, lvl);
                }
                c.AddMissleTTS.Add(id);
            }
        }

        private class AmmoBoxGenPattern : Pattern<AmmunitionBoxDef>
        {
            public Func<Match, ComponentOrder> Order = null;
            public bool Half = false, Double = false, Triple = false;
            public int ItemColAmount = -1;
            public override void Generate(AmmunitionBoxDef data, Match m, string targetFolder, string id, IdCollector c)
            {
                ComponentOrder o = Order(m);
                c.AddOrder(o, id);
                string icname = $"itemcollection_{data.AmmoID}";
                string itemcol = $"{icname},,,\r\n{data.Description.Id},AmmunitionBox,{ItemColAmount},10\r\n";
                if (Half)
                {
                    string nid = id + "_Half";
                    string b = MakeBox(data, nid, "(Half)", 0.5f, 1, 0.5f);
                    WriteTo(targetFolder, nid, b);
                    c.AddOrder(o, nid);
                    itemcol += $"{nid},AmmunitionBox,{ItemColAmount},10\r\n";
                }
                if (Double)
                {
                    string nid = id + "_Double";
                    string b = MakeBox(data, nid, "(Double)", 2, 2, 2.5f);
                    WriteTo(targetFolder, nid, b);
                    c.AddOrder(o, nid);
                    itemcol += $"{nid},AmmunitionBox,{ItemColAmount},10\r\n";
                }
                if (Triple)
                {
                    string nid = id + "_Triple";
                    string b = MakeBox(data, nid, "(Triple)", 3, 3, 4f);
                    WriteTo(targetFolder, nid, b);
                    c.AddOrder(o, nid);
                    itemcol += $"{nid},AmmunitionBox,{ItemColAmount},10\r\n";
                }
                if (ItemColAmount > 0)
                {
                    string icfol = Path.Combine(Path.GetDirectoryName(targetFolder), "itemcollections");
                    File.WriteAllText(Path.ChangeExtension(Path.Combine(icfol, icname), "csv"), itemcol);
                    c.ICReplace[id] = new ItemCollectionReplace()
                    {
                        Amount = 0,
                        Type = "Reference",
                        ID = icname,
                    };
                }
            }

            private string MakeBox(AmmunitionBoxDef data, string id, string nameadd, float tons, int slots, float capmul)
            {
                VersionManifestEntry e = data.DataManager.ResourceLocator.EntryByID(data.Description.Id, BattleTechResourceType.AmmunitionBoxDef);
                JObject json = JObject.Parse(File.ReadAllText(e.FilePath));

                int newcap = int.Parse(json["Capacity"].ToString());
                if (capmul > 0)
                    newcap = Mathf.CeilToInt(capmul * newcap);
                else
                    newcap = (int)capmul * newcap;

                json["Description"]["Id"] = id;
                string newname = $"{data.Description.UIName} {nameadd}";
                json["Description"]["UIName"] = newname;
                string desc = json["Description"]["Details"].ToString().Replace(data.Description.UIName, newname);
                desc = Regex.Replace(desc, "each contain [\\d]+(?<ty> Cluster Shot)? rounds and may feed multiple weapons", $"each contain {newcap}${{ty}} rounds and may feed multiple weapons");
                json["Description"]["Details"] = desc;
                json["InventorySize"] = slots;
                json["Tonnage"] = tons;
                json["Capacity"] = newcap;
                return json.ToString(Formatting.Indented);
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
