using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using Harmony;
using UnityEngine;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(LineOfSight), "GetLineOfFireUncached")]
    class LineOfSight_GetLineOfFireUncached
    {
        public static void Postfix(LineOfSight __instance, AbstractActor source, Vector3 sourcePosition, ICombatant target, ref LineOfFireLevel __result)
        {
            if (target is AbstractActor a && a.HasIndirectFireImmunity && __instance.GetVisibilityToTargetWithPositionsAndRotations(source, sourcePosition, a) != VisibilityLevel.LOSFull)
                __result = LineOfFireLevel.LOFBlocked;
        }
    }
}
