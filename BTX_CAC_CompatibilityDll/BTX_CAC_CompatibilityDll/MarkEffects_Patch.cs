using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    class MarkEffects_Patch
    {
        public static void Postfix(EffectData effectData, ICombatant target, List<Effect> __result)
        {
            if (effectData.Description.Id == "StatusEffect-NARC-IncomingAttBonus")
            {
                foreach (Effect e in __result)
                {
                    (target as AbstractActor)?.ProcessAddedMark(e);
                }
            }
        }
    }
}
