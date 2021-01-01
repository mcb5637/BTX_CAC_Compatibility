using BattleTech;
using Harmony;
using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[assembly:AssemblyVersion("0.1.3.0")]

namespace BTX_CAC_CompatibilityDll
{
    class Main
    {
        public static Settings Sett;
        public static ILog Log;

        public static void Init(string directory, string settingsJSON)
        {
            Log = HBS.Logging.Logger.GetLogger("BTX_CAC_Compatibility");
            try
            {
                Sett = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            }
            catch (Exception e)
            {
                Sett = new Settings();
                Log.LogException(e);
            }
            if (Sett.LogLevelLog)
                HBS.Logging.Logger.SetLoggerLevel("BTX_CAC_Compatibility", LogLevel.Log);
            HarmonyInstance harmony = HarmonyInstance.Create("com.github.mcb5637.BTX_CAC_Compatibility");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //harmony.Patch(AccessTools.DeclaredMethod(typeof(Mech), "DamageLocation"), new HarmonyMethod(AccessTools.Method(typeof(Main), "LogDamageLoc")), null, null);
            try
            {
                // artemis
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackDirector.AttackSequence), "GetClusteredHits"), "BEX.BattleTech.Extended_CE");
                Type[] ptypes = new Type[] { typeof(Vector3), typeof(Mech), typeof(float), typeof(ArmorLocation), typeof(float), typeof(float), typeof(ArmorLocation), typeof(float) };
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(HitLocation), "GetAdjacentHitLocation", ptypes), "BEX.BattleTech.Extended_CE");
                ptypes = new Type[] { typeof(Vector3), typeof(Vehicle), typeof(float), typeof(VehicleChassisLocations), typeof(float), typeof(float), typeof(VehicleChassisLocations), typeof(float) };
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(HitLocation), "GetAdjacentHitLocation", ptypes), "BEX.BattleTech.Extended_CE");
                // streak
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MissileLauncherEffect), "AllMissilesComplete"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MissileLauncherEffect), "LaunchMissile"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackDirector.AttackSequence), "GetIndividualHits"), "BEX.BattleTech.Extended_CE");
                // stepped hit chance
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ToHit), "GetUMChance"), "BEX.BattleTech.Extended_CE");
                // called shot nerf
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Pilot), "InitAbilities"), "BEX.BattleTech.Extended_CE", false, true, false);
            }
            catch (Exception e)
            {
                FileLog.Log(e.ToString());
            }
        }

        private static void Unpatch(HarmonyInstance harmony, MethodBase b, string id, bool pre=true, bool post=true, bool trans=true)
        {
            //FileLog.Log($"checking to unpatch: {b.DeclaringType.FullName}+{b.Name}");
            Patches pa = harmony.GetPatchInfo(b);
            if (pre)
                UnpatchCheckList(harmony, b, id, pa.Prefixes);
            if (post)
                UnpatchCheckList(harmony, b, id, pa.Postfixes);
            if (trans)
                UnpatchCheckList(harmony, b, id, pa.Transpilers);
        }

        private static void UnpatchCheckList(HarmonyInstance harmony, MethodBase b, string id, IEnumerable<Patch> pa)
        {
            foreach (Patch p in pa)
            {
                if (p.owner.Equals(id))
                {
                    MethodInfo patch = p.patch;
                    harmony.Unpatch(b, patch);
                    //FileLog.Log($"found: {patch.DeclaringType.FullName}+{patch.Name}");
                }
            }
        }

        //public static void LogDamageLoc(int originalHitLoc, ArmorLocation aLoc)
        //{
        //    FileLog.Log($"{originalHitLoc} -> {aLoc}");
        //}
    }
}
