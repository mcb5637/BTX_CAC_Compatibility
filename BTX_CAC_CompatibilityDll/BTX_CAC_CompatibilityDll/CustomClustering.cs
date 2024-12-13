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

        public float GetModifier(Weapon w, ICombatant target)
        {
            if (DeadfireBase >= 0.0f && w.Unguided())
                return DeadfireBase;
            if (ArtemisBase >= 0.0f && (IsArtemisMode(w) || ElectronicWarfare.DoesNARCApply(w, target)))
                return ArtemisBase;
            if (UAC != null)
            {
                int s = w.ShotsWhenFired;
                foreach (UACBase sm in UAC)
                {
                    if (sm.Shots == s)
                        return sm.Base;
                }
            }
            return Base;
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
            float f = clust.GetModifier(w, target);
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
    }
}
