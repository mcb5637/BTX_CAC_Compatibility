using BattleTech;
using CustAmmoCategories;
using CustomComponents;
using HarmonyLib;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    [CustomComponent("Clustering")]
    public class CustomClustering : SimpleCustomComponent
    {
        public enum ClusterStepType
        {
            None,
            Json,
            SRM,
            LRM,
            UAC,
        }
        public struct Modifier
        {
            public float Mod;
            public int GunnerySkill;
        }
        public class UACBase
        {
            public float Base = 1.0f;
            public int Shots = 1;
        }

        public Modifier[] Steps = null;
        public ClusterStepType StepType = ClusterStepType.Json;
        public float Base = -1.0f;
        public float DeadfireBase = -1.0f;
        public float ArtemisBase = -1.0f;
        public UACBase[] UAC = null;

        private IEnumerable<Modifier> StepsToUse()
        {
            IEnumerable<int> l = null;
            float m = 0.0f;
            switch (StepType)
            {
                case ClusterStepType.SRM:
                    l = AEPStatic.GetCESettings().TTSRMBonusSteps;
                    m = AEPStatic.GetCESettings().TTSRMStepSize;
                    break;
                case ClusterStepType.LRM:
                    l = AEPStatic.GetCESettings().TTLRMBonusSteps;
                    m = AEPStatic.GetCESettings().TTLRMStepSize;
                    break;
                case ClusterStepType.UAC:
                    l = AEPStatic.GetCESettings().TTRFBonusSteps;
                    m = AEPStatic.GetCESettings().TTRFStepSize;
                    break;
                case ClusterStepType.None:
                    return null;
                default:
                    return Steps;
            }
            return l.Select(x => new Modifier() { GunnerySkill = x, Mod = m / 100.0f });
        }

        private enum ModifierType
        {
            None,
            Missle,
            Deadfire,
            Artemis,
            Narc,
            UAC,
            LBX,
        }

        private ModifierType ModType(Weapon w, ICombatant target)
        {
            if (DeadfireBase >= 0.0f && w.Unguided())
                return ModifierType.Deadfire;
            if (ArtemisBase >= 0.0f)
            {
                if (IsArtemisMode(w))
                    return ModifierType.Artemis;
                if (ElectronicWarfare.DoesNARCApply(w, target))
                    return ModifierType.Narc;
            }
            if (UAC != null)
            {
                return ModifierType.UAC;
            }
            if (w.weaponDef.Type == WeaponType.Autocannon &&
                (w.weaponDef.WeaponSubType == WeaponSubType.LB2X || w.weaponDef.WeaponSubType == WeaponSubType.LB5X ||
                w.weaponDef.WeaponSubType == WeaponSubType.LB10X || w.weaponDef.WeaponSubType == WeaponSubType.LB20X))
                return ModifierType.LBX;
            return ModifierType.None;
        }

        public float GetBaseModifier(Weapon w, ICombatant target)
        {
            switch (ModType(w, target))
            {
                case ModifierType.Deadfire:
                    return DeadfireBase;
                case ModifierType.Artemis:
                case ModifierType.Narc:
                    return ArtemisBase;
                case ModifierType.UAC:
                    {
                        int s = w.ShotsWhenFired;
                        foreach (UACBase sm in UAC)
                        {
                            if (sm.Shots == s)
                                return sm.Base;
                        }
                        return Base;
                    }
                default:
                    return Base;
            }
        }

        internal static bool IsArtemisMode(Weapon w)
        {
            return w.mode().Id.EndsWith("RM_A4");
        }

        public static float GetModifierForWeapon(Weapon w, int gunnery, ICombatant target)
        {
            if (w.ShotsWhenFired <= 1)
                return 1.0f;
            CustomClustering clust = w.weaponDef.GetComponent<CustomClustering>();
            if (clust == null)
                return 1.0f;
            float f = clust.GetBaseModifier(w, target);
            IEnumerable<Modifier> steps = clust.StepsToUse();
            if (f <= 0.0f)
                return 1.0f;
            if (steps == null)
                return f;
            foreach (Modifier mo in steps)
            {
                if (mo.GunnerySkill <= gunnery)
                    f += mo.Mod;
            }
            return f;
        }

        public static float Cluster_Multiplier(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            return GetModifierForWeapon(wep, attacker.GetPilot()?.Gunnery ?? 0, target);
        }

        internal static string Cluster_EffectName(ToHit h, AbstractActor a, Weapon w, ICombatant t, Vector3 ap, Vector3 tp, LineOfFireLevel lof, MeleeAttackType mt, bool cs)
        {
            CustomClustering clust = w.weaponDef.GetComponent<CustomClustering>();
            if (clust == null)
                return "Clustering Modifier";
            switch (clust.ModType(w, t))
            {
                case ModifierType.Missle:
                    return "MISSILE MODIFIER";
                case ModifierType.Deadfire:
                    return "DF MISSILE MOD";
                case ModifierType.Artemis:
                    return "MISSILE MOD ARTIV";
                case ModifierType.Narc:
                    return "MISSILE MOD NARC";
                case ModifierType.UAC:
                    return "RAPID FIRE MOD";
                case ModifierType.LBX:
                    return "MUNITION MODIFIER";
                default:
                    return "Clustering Modifier";
            }
        }
    }
}
