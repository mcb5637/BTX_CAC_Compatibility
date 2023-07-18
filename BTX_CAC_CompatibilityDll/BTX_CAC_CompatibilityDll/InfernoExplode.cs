using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using HarmonyLib;
using CustAmmoCategories;

namespace BTX_CAC_CompatibilityDll
{
    class InfernoExplode
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.DeclaredMethod(typeof(AmmunitionBox), "DamageComponent"), new HarmonyMethod(AccessTools.Method(typeof(InfernoExplode), "Prefix"))
            {
                before = new string[] { "BEX.BattleTech.Extended_CE" }
            }, null, null);
        }

        public static bool Prefix(AmmunitionBox __instance, ComponentDamageLevel damageLevel, bool applyEffects, WeaponHitInfo hitInfo)
        {
            if (damageLevel == ComponentDamageLevel.Destroyed && applyEffects && __instance.componentDef.CanExplode && __instance.componentDef.ComponentTags.Contains("component_infernoExplosion"))
            {
                if (__instance.parent is Mech m)
                {
                    AmmunitionBoxDef b = __instance.componentDef as AmmunitionBoxDef;
                    int dmg = (int) b.Ammo.extDef().HeatDamagePerShot;
                    int ammoleft = __instance.StatCollection.GetValue<int>("CurrentAmmo");
                    m.AddExternalHeat("inferno explosion", dmg * ammoleft / 2);
                    foreach (EffectData e in b.Ammo.extDef().statusEffects.Where((e) => e.effectType == EffectType.StatisticEffect))
                    {
                        m.Combat.EffectManager.CreateEffect(e, e.Description.Id, hitInfo.attackSequenceId, m, m, default, -1, false);
                    }
                    m.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId)?.FlagAttackDidHeatDamage(m.GUID);
                }
            }
            return true;
        }
    }
}
