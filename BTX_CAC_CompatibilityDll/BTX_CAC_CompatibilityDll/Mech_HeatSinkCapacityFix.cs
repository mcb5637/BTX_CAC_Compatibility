using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using AccessExtension;
using BattleTech;
using CustAmmoCategoriesPatches;
using Harmony;

namespace BTX_CAC_CompatibilityDll
{
    //[HarmonyPatch(typeof(Mech), "HeatSinkCapacity", MethodType.Getter)]
    public static class Mech_HeatSinkCapacityFix
    {
        //public static void Postfix(Mech __instance, ref float __result)
        //{
        //    __result *= __instance.StatCollection.GetValue<float>("HeatSinkCapacityMult");
        //}

        public static float GetHeatSinkMul(this Mech m)
        {
            return m.StatCollection.GetValue<float>("HeatSinkCapacityMult"); ;
        }

        public static void Patch(HarmonyInstance h)
        {
            h.Patch(AccessTools.DeclaredMethod(typeof(Mech_HeatSinkCapacity), "Postfix"), null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Mech_HeatSinkCapacityFix), "Transpiler_CAC_AdjustedHeatsinkCapacity")));
            h.Patch(AccessTools.DeclaredMethod(typeof(Mech_ApplyHeatSinks), "Prefix"), null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Mech_HeatSinkCapacityFix), "Transpiler_CAC_ApplyHeatsinks")));
        }

        public static IEnumerable<CodeInstruction> Transpiler_CAC_AdjustedHeatsinkCapacity(IEnumerable<CodeInstruction> code)
        {
            return AccessExtensionPatcher.TranspilerHelper(code, new CodeInstruction[] {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(AbstractActor_OnActivationBegin), "isUsedHeatSinkReseted"))
                }, (f) =>
                {
                    CodeInstruction i = f.First.Value;
                    f.RemoveFirst();
                    i.opcode = OpCodes.Ldloc_0;
                    return new CodeInstruction[]
                    {
                        i,
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Mech_HeatSinkCapacityFix), "GetHeatSinkMul")),
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stloc_0),

                        new CodeInstruction(OpCodes.Ldarg_0), // prev
                    };
                });
        }
        public static IEnumerable<CodeInstruction> Transpiler_CAC_ApplyHeatsinks(IEnumerable<CodeInstruction> code)
        {
            return AccessExtensionPatcher.TranspilerHelper(code, new CodeInstruction[] {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(HeatConstantsDef), "GlobalHeatSinkMultiplier")),
                    new CodeInstruction(OpCodes.Mul),
                }, (f) =>
                {
                    f.Clear();
                    return new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(HeatConstantsDef), "GlobalHeatSinkMultiplier")),
                        new CodeInstruction(OpCodes.Mul),

                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Mech_HeatSinkCapacityFix), "GetHeatSinkMul")),
                        new CodeInstruction(OpCodes.Mul),
                    };
                });
        }
    }
}
