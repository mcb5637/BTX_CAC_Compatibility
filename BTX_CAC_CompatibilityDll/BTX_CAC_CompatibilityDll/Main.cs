using AccessExtension;
using BattleTech;
using BattleTech.UI;
using CustAmmoCategories;
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

[assembly: AssemblyVersion("0.1.19.0")]

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
            AccessExtensionPatcher.PatchAll(harmony, Assembly.GetExecutingAssembly());
            AbstractActor_InitStats.Patch(harmony);
            AbstractActor_IndirectImmune.Patch(harmony);
            //harmony.Patch(
            //    AccessTools.Method(typeof(EffectManager), "CreateEffect", new Type[] { typeof(EffectData), typeof(string), typeof(int), typeof(ICombatant), typeof(ICombatant), typeof(WeaponHitInfo), typeof(int), typeof(bool) }),
            //    null, new HarmonyMethod(AccessTools.Method(typeof(MarkEffects_Patch), "Postfix")), null);
            //harmony.Patch(AccessTools.DeclaredMethod(typeof(Mech), "DamageLocation"), new HarmonyMethod(AccessTools.Method(typeof(Main), "LogDamageLoc")), null, null);


            if (Sett.MECompat)
                return;

            InfernoExplode.Patch(harmony);

            try
            {
                // artemis
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackDirector.AttackSequence), "GetClusteredHits"), "BEX.BattleTech.Extended_CE");
                //Type[] ptypes = new Type[] { typeof(Vector3), typeof(Mech), typeof(float), typeof(ArmorLocation), typeof(float), typeof(float), typeof(ArmorLocation), typeof(float) };
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(HitLocation), "GetAdjacentHitLocation", ptypes), "BEX.BattleTech.Extended_CE");
                //ptypes = new Type[] { typeof(Vector3), typeof(Vehicle), typeof(float), typeof(VehicleChassisLocations), typeof(float), typeof(float), typeof(VehicleChassisLocations), typeof(float) };
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(HitLocation), "GetAdjacentHitLocation", ptypes), "BEX.BattleTech.Extended_CE");
                // streak
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MissileLauncherEffect), "AllMissilesComplete"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MissileLauncherEffect), "LaunchMissile"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackDirector.AttackSequence), "GetIndividualHits"), "BEX.BattleTech.Extended_CE");
                // stepped hit chance
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ToHit), "GetUMChance"), "BEX.BattleTech.Extended_CE");
                // called shot nerf
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Pilot), "InitAbilities"), "BEX.BattleTech.Extended_CE", false, true, false);
                // TSM
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Mech), "OnActivationEnd"), "BEX.BattleTech.Extended_CE", false, true, false); // prefix is heat shutdown
                // lbx/uac
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Contract), "CompleteContract"), "BEX.BattleTech.Extended_CE", true, true, true,
                    "System.Void Extended_CE.WeaponModes+Contract_CompleteContract.Prefix(BattleTech.Contract __instance)");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackEvaluator), "MakeAttackOrder"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AITeam), "TurnActorProcessActivation"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(CombatHUDWeaponSlot), "OnPointerUp"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Weapon), "get_AmmoCategoryValue"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(CombatHUDWeaponSlot), "RefreshDisplayedWeapon"), "BEX.BattleTech.Extended_CE", true, false, false); // transpiler is coil fix
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechValidationRules), "ValidateMechHasAppropriateAmmo"), "BEX.BattleTech.Extended_CE");
            }
            catch (Exception e)
            {
                FileLog.Log(e.ToString());
            }

            try
            {
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (a.GetName().Name.Equals("MechQuirks"))
                    {
                        Type t = a.GetType("Quirks.Tooltips.QuirkToolTips");
                        harmony.Patch(t.GetMethod("DetailMechQuirks", BindingFlags.Static | BindingFlags.NonPublic), null, new HarmonyMethod(typeof(QuirkToolTips_DetailMechQuirks), "Postfix"), null);
                    }
                }
            }
            catch (Exception e)
            {
                FileLog.Log(e.ToString());
            }

            ToHitModifiersHelper.registerModifier("BTX_CAC_Compatibility_CounterNarc", "Counter NARC", true, true, CounterNarc, null);
        }

        private static float CounterNarc(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            AbstractActor at = target as AbstractActor;
            if (at != null && at.HasIndirectFireImmunity && at.Combat.EffectManager.GetAllEffectsTargetingWithBaseID(at, "StatusEffect-NARC-IncomingAttBonus").Count > 0)
            {
                return 3;
            }
            return 0;
        }

        private static void Unpatch(HarmonyInstance harmony, MethodBase b, string id, bool pre=true, bool post=true, bool trans=true, string onlyUnpatch=null)
        {
            //FileLog.Log($"checking to unpatch: {b.FullName()}");
            Patches pa = harmony.GetPatchInfo(b);
            if (pa==null)
            {
                //FileLog.Log("no patch attached");
                return;
            }
            if (pre)
                UnpatchCheckList(harmony, b, id, pa.Prefixes, onlyUnpatch);
            if (post)
                UnpatchCheckList(harmony, b, id, pa.Postfixes, onlyUnpatch);
            if (trans)
                UnpatchCheckList(harmony, b, id, pa.Transpilers, onlyUnpatch);
        }

        private static void UnpatchCheckList(HarmonyInstance harmony, MethodBase b, string id, IEnumerable<Patch> pa, string onlyUnpatch)
        {
            foreach (Patch p in pa)
            {
                MethodInfo patch = p.patch;
                if (p.owner.Equals(id) && (onlyUnpatch==null || onlyUnpatch.Equals(patch.FullName())))
                {
                    harmony.Unpatch(b, patch);
                    //FileLog.Log($"found: {patch.FullName()}");
                }
            }
        }

        //public static void LogDamageLoc(int originalHitLoc, ArmorLocation aLoc)
        //{
        //    FileLog.Log($"{originalHitLoc} -> {aLoc}");
        //}
    }
}
