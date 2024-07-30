using BattleTech;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Mech), nameof(Mech.MaxMeleeEngageRangeDistance), MethodType.Getter)]
    public static class Mech_MaxMeleeEngageRangeDistance
    {
        public static void Postfix(Mech __instance, ref float __result)
        {
            if (__instance.CanShootAfterSprinting)
                __result = __instance.MaxSprintDistance;
        }
    }
    [HarmonyPatch]
    public class Pathing_MeleeGrid
    {
        private static PathNodeGrid GetMeleeGrid(Pathing __instance)
        {
            return __instance.OwningActor.CanShootAfterSprinting ? __instance.SprintingGrid() : __instance.WalkingGrid();
        }

        [HarmonyPatch(typeof(Pathing), nameof(Pathing.getGrid))]
        [HarmonyPatch(typeof(Pathing), nameof(Pathing.GetMeleeDestsForTarget))]
        [HarmonyPatch(typeof(Pathing), nameof(Pathing.SetMeleeTarget))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var prev = AccessTools.PropertyGetter(typeof(Pathing), "MeleeGrid");
            var rep = AccessTools.Method(typeof(Pathing_MeleeGrid), nameof(GetMeleeGrid));
            foreach (var c in instructions)
            {
                if (c.operand as MethodInfo == prev)
                    c.operand = rep;
                yield return c;
            }
        }
    }
}
