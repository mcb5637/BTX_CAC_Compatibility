using BattleTech;
using CustAmmoCategories;
using CustomComponents;
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
        public class Modifier
        {
            public float Mod = 1.0f;
            public int GunnerySkill = 0;
        }
        public class UACBase
        {
            public float Base = 1.0f;
            public int Shots = 1;
        }

        public Modifier[] Steps = null;
        public float Base = -1.0f;
        public float DeadfireBase = -1.0f;
        public float ArtemisBase = -1.0f;
        public UACBase[] UAC = null;

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
            Modifier[] steps = clust.Steps;
            if (f <= 0.0f)
                return 1.0f;
            if (steps == null || steps.Length == 0)
                return f;
            foreach (Modifier mo in steps)
            {
                if (mo.GunnerySkill <= gunnery && f < mo.Mod)
                    f = mo.Mod;
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
