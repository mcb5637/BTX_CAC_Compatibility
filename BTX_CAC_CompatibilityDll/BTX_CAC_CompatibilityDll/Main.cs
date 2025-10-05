using AccessExtension;
using BattleTech;
using BattleTech.Save;
using BattleTech.StringInterpolation;
using BattleTech.UI;
using BiggerDrops.Features;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomActivatableEquipment;
using CustomUnits;
using Extended_CE;
using Extended_CE.Functionality;
using HarmonyLib;
using HBS.Logging;
using InControl;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UIWidgets;
using UnityEngine;
using static Extended_CE.BTComponents;

[assembly: AssemblyVersion("2.0.3.0")]

namespace BTX_CAC_CompatibilityDll
{
    class Main
    {
        public static Settings Sett = new Settings();
        public static ILog Log;
        public static string Directory;
        public static Dictionary<string, WeaponAddonSplit> Splits = new Dictionary<string, WeaponAddonSplit>();

        public static void Init(string directory, string settingsJSON)
        {
            Log = HBS.Logging.Logger.GetLogger("BTX_CAC_Compatibility");
            Directory = directory;
            try
            {
                Sett = JsonConvert.DeserializeObject<Settings>(settingsJSON);
                ItemCollectionDef_FromCSV.Replaces = JsonConvert.DeserializeObject<Dictionary<string, ItemCollectionReplace>>(File.ReadAllText(Path.Combine(directory, "automerge", "itemcollectionreplace.json")));
                foreach (KeyValuePair<string, ItemCollectionReplace> kv in JsonConvert.DeserializeObject<Dictionary<string, ItemCollectionReplace>>(File.ReadAllText(Path.Combine(directory, "itemcollectionreplace.json"))))
                {
                    ItemCollectionDef_FromCSV.Replaces[kv.Key] = kv.Value;
                }
                Splits = JsonConvert.DeserializeObject<Dictionary<string, WeaponAddonSplit>>(File.ReadAllText(Path.Combine(directory, "automerge", "addonsplit.json")));
                foreach (KeyValuePair<string, WeaponAddonSplit> kv in JsonConvert.DeserializeObject<Dictionary<string, WeaponAddonSplit>>(File.ReadAllText(Path.Combine(directory, "addonsplit.json"))))
                {
                    Splits[kv.Key] = kv.Value;
                }
            }
            catch (Exception e)
            {
                Log.LogException(e);
            }
            if (Sett.LogLevelLog)
                HBS.Logging.Logger.SetLoggerLevel("BTX_CAC_Compatibility", LogLevel.Log);
            Harmony harmony = new Harmony("com.github.mcb5637.BTX_CAC_Compatibility");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            AccessExtensionPatcher.PatchAll(harmony, Assembly.GetExecutingAssembly());
            CU2ComponentFix.Patch(harmony);
            if (Sett.FixDropslotsInOldSaves)
                SimGameState_InitStats.Patch(harmony);
            //harmony.Patch(
            //    AccessTools.Method(typeof(EffectManager), "CreateEffect", new Type[] { typeof(EffectData), typeof(string), typeof(int), typeof(ICombatant), typeof(ICombatant), typeof(WeaponHitInfo), typeof(int), typeof(bool) }),
            //    null, new HarmonyMethod(AccessTools.Method(typeof(MarkEffects_Patch), "Postfix")), null);
            //harmony.Patch(AccessTools.DeclaredMethod(typeof(Mech), "DamageLocation"), new HarmonyMethod(AccessTools.Method(typeof(Main), "LogDamageLoc")), null, null);


            if (Sett.MECompat)
                return;

            InfernoExplode.Patch(harmony);
            MechAutoFixer.Register();
            MovableBlockers.RegisterValidators();
            ComponentUpgrader.Register();

            try
            {
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
                // masc/ams json modify
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(UpgradeDef), "FromJSON"), "BEX.BattleTech.Extended_CE");
                // ecm
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ChassisDef), "FromJSON"), "BEX.BattleTech.Extended_CE");
#if !DEBUG
                // tt ranges, remains in debug, so autopatcher can read modified values ;)
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(WeaponDef), "FromJSON"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AmmunitionBoxDef), "FromJSON"), "BEX.BattleTech.Extended_CE");
#endif
                // cluster
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ToHit), nameof(ToHit.GetToHitChance)), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ToHit), nameof(ToHit.GetAllModifiers)), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ToHit), nameof(ToHit.GetAllMeleeModifiers)), "BEX.BattleTech.Extended_CE");

                // movement
                Unpatch(harmony, AccessTools.DeclaredPropertyGetter(typeof(Mech), nameof(Mech.CanSprint)), "BEX.BattleTech.Extended_CE");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackEvaluator), "MakeAttackOrder"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AITeam), "TurnActorProcessActivation"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(CombatHUDWeaponSlot), "OnPointerUp"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Weapon), "get_AmmoCategoryValue"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(CombatHUDWeaponSlot), "RefreshDisplayedWeapon"), "BEX.BattleTech.Extended_CE", true, false, false); // transpiler is coil fix
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechValidationRules), "ValidateMechHasAppropriateAmmo"), "BEX.BattleTech.Extended_CE");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechStatisticsRules), "CalculateTonnage"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechLabMechInfoWidget), "CalculateTonnage"), "BEX.BattleTech.Extended_CE");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(InventoryItemElement_NotListView), "OnDestroy"), "com.github.m22spencer.BattletechPerformanceFix");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechValidationRules), "ValidatePrototypeEquipment"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechValidationRules), "ValidateMechDef"), "BEX.BattleTech.Extended_CE");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Mech), "InitStats"), "BattleTech.Haree.BEXTimeline");
            }
            catch (Exception e)
            {
                Log.LogError(e.ToString());
            }

            try
            {
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (a.GetName().Name.Equals("MechQuirks"))
                    {
                        Type t = a.GetType("Quirks.Tooltips.QuirkToolTips");
                        harmony.Patch(t.GetMethod("DetailMechQuirks", BindingFlags.Static | BindingFlags.NonPublic), null, new HarmonyMethod(typeof(QuirkToolTips_DetailMechQuirks), "Postfix"), null);
                        harmony.Patch(t.GetMethod("DetailMechQuirksBuild", BindingFlags.Static | BindingFlags.NonPublic), null, new HarmonyMethod(typeof(QuirkToolTips_DetailMechQuirksBuild), "Postfix"), null);
                    }
                }
                //harmony.Patch(AccessTools.GetDeclaredConstructors(typeof(IndexOutOfRangeException)).Single((x)=>x.GetParameters().Length==0), null, new HarmonyMethod(typeof(Main), "ExCtorLog"));
            }
            catch (Exception e)
            {
                Log.LogError(e.ToString());
            }

            ToHitModifiersHelper.registerModifier("BTX_CAC_Compatibility_ECM", "ECM", true, true, ElectronicWarfare.ECM_Effect, null);
            ToHitModifiersHelper.registerModifier("LIGHT", "LIGHT", true, false, LightWeatherEffects.Light_Effect, null);
            ToHitModifiersHelper.registerModifier("ILLUMINATED", "HEAT ILLUMINATED", true, false, LightWeatherEffects.Illuminated_Effect, null);
            ToHitModifiersHelper.registerModifier("TRACER", "TRACER", true, false, LightWeatherEffects.Tracer_Effect, null);
            ToHitModifiersHelper.registerModifier("MOVED SELF", "MOVED SELF", false, false, MovementRework.MovedSelf_Effect, MovementRework.MovedSelf_EffectName);
            ToHitModifiersHelper.registerModifier("TAGNARC", "TAGNARC", true, false, ElectronicWarfare.NARC_TAG_Effect, ElectronicWarfare.NARC_TAG_EffectName);
            ToHitModifiersHelper.registerModifier("A4 TAG", "NO TAG", true, false, A4_Tag_Effect, null);
            ToHitModifiersHelper.registerModifier("ROLE", "ROLE", true, false, UnitRoles.Role_Effect, UnitRoles.Role_EffectName);
            ToHitModifiersHelper.multipliers["CLUSTER"] = new ToHitModifier("CLUSTER", "CLUSTER", true, false, CustomClustering.Cluster_Multiplier, CustomClustering.Cluster_EffectName, null);
            ToHitModifiersHelper.modifiers.Remove("SPRINTED");
            CustomComponents.Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
            CustomComponents.Validator.RegisterMechValidator(TTS.MechValidator, TTS.MechValidatorFieldable);
            MoveStatusPreview_DisplayPreviewStatus.MoveTypeDisplayOverride = MovementRework.MoveTypeDisplayOverride;
        }

        internal static float A4_Tag_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            if (wep.ammo().Id == "Ammunition_Arrow4_Homing")
            {
                if (target.StatCollection.GetValue<float>("TAGCount") + target.StatCollection.GetValue<float>("TAGCountClan") <= 0.0f)
                    return 100.0f;
            }
            return 0.0f;
        }

        private static void Unpatch(Harmony harmony, MethodBase b, string id, bool pre = true, bool post = true, bool trans = true, string onlyUnpatch = null)
        {
            //FileLog.Log($"checking to unpatch: {b.FullName()} {pre} {post} {trans}");
            Patches pa = Harmony.GetPatchInfo(b);
            if (pa == null)
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

        private static void UnpatchCheckList(Harmony harmony, MethodBase b, string id, IEnumerable<Patch> pa, string onlyUnpatch)
        {
            foreach (Patch p in pa)
            {
                MethodInfo patch = p.PatchMethod;
                if (p.owner.Equals(id) && (onlyUnpatch == null || onlyUnpatch.Equals(patch.FullName())))
                {
                    harmony.Unpatch(b, patch);
                    //FileLog.Log($"found: {patch.FullName()}");
                }
                else
                {
                    //FileLog.Log($"no match: {patch.FullName()} {p.owner}");
                }
            }
        }

        public static void ExCtorLog(Exception __instance)
        {
            string t = new StackTrace().ToString();
            Log.Log(t);
        }

        //public static void LogDamageLoc(int originalHitLoc, ArmorLocation aLoc)
        //{
        //    FileLog.Log($"{originalHitLoc} -> {aLoc}");
        //}
    }

    [HarmonyPatch]
    public class Mech_InitStats_MASCFix
    {
        [HarmonyPatch(typeof(Mech), "InitStats")]
        [HarmonyAfter("BEX.BattleTech.Extended_CE")]
        public static void Postfix(Mech __instance)
        {
            if (BTComponents.MechTTRuleInfo.MechTTStatStore.TryGetValue(__instance.uid, out BTComponents.TTRuleInfo i))
                i.OperatingMASC = false;
        }
    }

    [HarmonyPatch]
    public class Chassis_MovementCapDef_Fix
    {
        [HarmonyPatch(typeof(ChassisDef), nameof(ChassisDef.MovementCapDef), MethodType.Getter)]
        public static void Postfix(ChassisDef __instance, ref MovementCapabilitiesDef __result)
        {
            if (__result == null)
            {
                if (__instance.DataManager == null)
                    __instance.DataManager = UnityGameInstance.BattleTechGame.DataManager;
                if (__instance.DataManager != null && __instance.DataManager.MovementCapabilitiesDefs.Exists(__instance.MovementCapDefID))
                {
                    __instance.Refresh();
                    __result = __instance.MovementCapDef;
                }
            }
        }
    }
    [HarmonyPatch]
    public class RandomPatches
    {
        private static void Clear(List<string> l)
        {
            l?.Clear();
        }
        [HarmonyPatch(typeof(BEXTimeline.UpdateOwnership), nameof(BEXTimeline.UpdateOwnership.UpdateTheMap))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateTheMapNullref(IEnumerable<CodeInstruction> inst)
        {
            MethodInfo ori = AccessTools.Method(typeof(List<string>), nameof(List<string>.Clear));
            MethodInfo rep = AccessTools.Method(typeof(RandomPatches), nameof(Clear));
            foreach (CodeInstruction i in inst)
            {
                if ((i.opcode == OpCodes.Call || i.opcode == OpCodes.Callvirt) && (i.operand as MethodInfo) == ori)
                {
                    i.opcode = OpCodes.Call;
                    i.operand = rep;
                }

                yield return i;
            }
        }

        private static float GetFlexDamage(Weapon w)
        {
            ExtAmmunitionDef a = w.ammo();
            if (a.Id == "Ammunition_SRMInferno")
            {
                float bon = w.weaponDef.Damage - 10.0f;
                if (bon > 0)
                    return bon;
            }
            return 0.0f;
        }
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.HeatDamagePerShot), MethodType.Getter)]
        [HarmonyPostfix]
        [HarmonyBefore("io.mission.modrepuation")]
        public static void Weapon_HeatDamagePerShot(Weapon __instance, ref float __result)
        {
            float f = GetFlexDamage(__instance);
            if (f > 0.0f)
                __result += f;
        }
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.DamagePerShot), MethodType.Getter)]
        [HarmonyPostfix]
        [HarmonyBefore("io.mission.modrepuation")]
        public static void Weapon_DamagePerShot(Weapon __instance, ref float __result)
        {
            float f = GetFlexDamage(__instance);
            if (f > 0.0f)
                __result -= f;
        }

        private static void AttackComplete(EffectManager mng, ICombatant tar, string sourceId)
        {
            var sour = tar.Combat.FindActorByGUID(sourceId);
            if (sour != null && ReferenceEquals(sour, tar))
                return;
            mng.OnAttackComplete(tar);
        }
        [HarmonyPatch(typeof(Mech), nameof(Mech.ResolveAttackSequence))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Mech_ResolveAttackSequence(IEnumerable<CodeInstruction> inst)
        {
            var orig = AccessTools.Method(typeof(EffectManager), nameof(EffectManager.OnAttackComplete));
            var rep = AccessTools.Method(typeof(RandomPatches), nameof(AttackComplete));
            foreach (var c in inst)
            {
                if ((c.opcode == OpCodes.Call || c.opcode == OpCodes.Callvirt) && c.operand as MethodInfo == orig)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    c.operand = rep;
                }
                yield return c;
            }
        }

        [HarmonyPatch(typeof(Contract), "ResetStateForRestart")]
        [HarmonyPostfix]
        public static void Contract_ResetStateForRestart()
        {
            foreach (KeyValuePair<string, TTRuleInfo> entry in MechTTRuleInfo.MechTTStatStore)
            {
                entry.Value.HipCrits = 0;
                entry.Value.EngineCrits = 0;
                entry.Value.EngineCenterCrits = 0;
                entry.Value.EngineLeftCrits = 0;
                entry.Value.EngineRightCrits = 0;
                entry.Value.GyroDestroyed = false;
                entry.Value.LifeSupportCrit = false;
            }
        }

        [HarmonyPatch(typeof(Contract), "CompleteContract")]
        [HarmonyPrefix]
        public static void Contract_CompleteContract(Contract __instance)
        {
            if (__instance.State != Contract.ContractState.InProgress) return;
            __instance.BattleTechGame.Combat.AllActors.OfType<Mech>().Where(mech => FakeDatabase.IsVehicle(mech.MechDef)).ToList()
                .ForEach(vehicle => { vehicle.allComponents.RemoveAll(comp => comp.defId == "Gear_BEX_MotiveSystem"); vehicle.miscComponents.RemoveAll(comp => comp.defId == "Gear_BEX_MotiveSystem"); });
        }

        [HarmonyPatch(typeof(ChassisDef), "FromJSON")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void ChassisDef_FromJSON(ChassisDef __instance)
        {
            if (FakeDatabase.IsVehicle(__instance))
            {
                __instance.SetMeleeDamage(0);
            }
        }

        private static Mech MechNullIfVehicle(Mech m)
        {
            if (m != null && m.FakeVehicle())
                return null;
            return m;
        }
        [HarmonyPatch(typeof(AttackEvaluator), "MakeAttackOrderForTarget")]
        [HarmonyTranspiler]
        //[HarmonyEmitIL]
        public static IEnumerable<CodeInstruction> AttackEvaluator_MakeAttackOrderForTarget(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var cm = new CodeMatcher(instructions, il);
            cm.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(x => x.opcode == OpCodes.Brtrue_S || x.opcode == OpCodes.Brtrue),

                new CodeMatch(x => x.opcode == OpCodes.Ldloc || x.opcode == OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(x => x.opcode == OpCodes.Beq_S || x.opcode == OpCodes.Beq),

                new CodeMatch(x => x.opcode == OpCodes.Ldloc || x.opcode == OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(x => x.opcode == OpCodes.Bne_Un_S || x.opcode == OpCodes.Bne_Un),

                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(x => x.opcode == OpCodes.Ldstr && ((string)x.operand == "this unit can't melee or dfa"))
            );
            cm.ThrowIfInvalid("failed to find if statement");

            cm.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomPatches), nameof(MechNullIfVehicle))));
            
            return cm.InstructionEnumeration();
        }

        private static bool ComponentsAndNotVehicle(Mech mech) => Extended_CE.Core.UsingComponents() && !mech.FakeVehicle();
        [HarmonyPatch(typeof(BTComponents.Mech_InitStats), "Postfix")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Mech_InitStats_Postfix(IEnumerable<CodeInstruction> inst)
        {
            var orig = AccessTools.Method(typeof(Extended_CE.Core), nameof(Extended_CE.Core.UsingComponents));
            var rep = AccessTools.Method(typeof(RandomPatches), nameof(ComponentsAndNotVehicle));
            foreach (var c in inst)
            {
                if ((c.opcode == OpCodes.Call || c.opcode == OpCodes.Callvirt) && c.operand as MethodInfo == orig)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    c.operand = rep;
                }
                yield return c;
            }
        }
    }
}
