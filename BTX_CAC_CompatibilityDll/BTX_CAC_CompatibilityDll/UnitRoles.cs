using BattleTech;
using CustomUnits;
using Extended_CE.Functionality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    internal class UnitRoles
    {
        internal static float Role_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            if (!TacticalGameChanges.UnitRoleStore.TryGetValue(attacker.uid, out Extended_CE.UnitRole attacker_role))
                return 0.0f;
            switch (attacker_role)
            {
                case Extended_CE.UnitRole.Scout:
                    {
                        if (!TacticalGameChanges.UnitRoleStore.TryGetValue(target.uid, out Extended_CE.UnitRole target_role))
                            return 0.0f;
                        return target_role == Extended_CE.UnitRole.Scout ? -1.0f : 0.0f;
                    }
                case Extended_CE.UnitRole.Striker:
                    {
                        bool v = target is Vehicle;
                        bool t = target is Turret;
                        if (target is CustomMech cm)
                        {
                            v = cm.isVehicle;
                            t = cm.isTurret;
                        }
                        if (v || target is Building || t)
                            return -1.0f;
                        return 0.0f;
                    }
                case Extended_CE.UnitRole.Ambusher:
                    return TacticalGameChanges.AmbushersYetToFire.Contains(attacker.uid) ? -2.0f : 0.0f;
                case Extended_CE.UnitRole.Skirmisher:
                    return MovementRework.ClassifyMoved(attacker, apos) == SelfMovedModifier.Run ? -1.0f : 0.0f;
                case Extended_CE.UnitRole.Sniper:
                    return MovementRework.MovedSelfModifier_Fallback(attacker, apos) <= 0.0f ? -1.0f : 0.0f;
                default:
                    return 0.0f;
            }
        }
        internal static string Role_EffectName(ToHit h, AbstractActor a, Weapon w, ICombatant t, Vector3 ap, Vector3 tp, LineOfFireLevel lof, MeleeAttackType mt, bool cs)
        {
            if (!TacticalGameChanges.UnitRoleStore.TryGetValue(a.uid, out Extended_CE.UnitRole attacker_role))
                return "ROLE";
            return attacker_role.ToString().ToUpper();
        }
    }
}
