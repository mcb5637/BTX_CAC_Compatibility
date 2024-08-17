using AccessExtension;
using BattleTech;
using CustomUnits;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    public class CU2ComponentFix
    {
        public static bool UsingComponents(Mech m)
        {
            if (m != null && m is CustomMech cm)
            {
                if (cm.isSquad || cm.isVehicle)
                    return false;
            }
            return Extended_CE.Core.UsingComponents();
        }

        public static void Patch(Harmony h)
        {
            try
            {
                Assembly a = AccessExtensionPatcher.GetLoadedAssemblyByName("Extended_CE");
                // contract completecontract not needed to patch?
                Type mechcomp_dmg = a.GetType("Extended_CE.BTComponents+MechComponent_DamageComponent");
                h.Patch(AccessTools.DeclaredMethod(mechcomp_dmg, "Postfix"), null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(Transpiler_DamageComponent))));
                h.Patch(AccessTools.DeclaredMethod(mechcomp_dmg, "Prefix"), null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(Transpiler_DamageComponentPre))));
                // mech checkforcrit not needed to patch?
                // mech getcomponentinslot not needed to patch?
                Type Mech_initstats = a.GetType("Extended_CE.BTComponents+Mech_InitStats");
                h.Patch(AccessTools.DeclaredMethod(Mech_initstats, "Prefix"), null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(Transpiler_MechInitStats))));
                h.Patch(AccessTools.DeclaredMethod(Mech_initstats, "Postfix"), null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(Transpiler_MechInitStats))));
                // mech update min stability not needed to patch?
                // weapon init stats only melee, so vehicles should be safe
                FixMech_InitStats_PatchOrder(h);
            }
            catch (Exception e)
            {
                Main.Log.LogError(e.ToString());
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler_MechInitStats(IEnumerable<CodeInstruction> code)
        {
            return AccessExtensionPatcher.TranspilerHelper(code, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Extended_CE.Core), nameof(UsingComponents))),
            }, (prev) =>
            {
                CodeInstruction ins = prev.First.Value;
                ins.operand = null;
                ins.opcode = OpCodes.Ldarg_0;
                prev.Clear();
                return new CodeInstruction[]
                {
                    ins,
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(UsingComponents))),
                };
            });
        }
        public static IEnumerable<CodeInstruction> Transpiler_DamageComponent(IEnumerable<CodeInstruction> code)
        {
            return AccessExtensionPatcher.TranspilerHelper(code, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Extended_CE.Core), nameof(UsingComponents))),
            }, (prev) =>
            {
                CodeInstruction ins = prev.First.Value;
                ins.operand = null;
                ins.opcode = OpCodes.Ldloc_0;
                prev.Clear();
                return new CodeInstruction[]
                {
                    ins,
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(UsingComponents))),
                };
            });
        }
        public static IEnumerable<CodeInstruction> Transpiler_DamageComponentPre(IEnumerable<CodeInstruction> code)
        {
            return AccessExtensionPatcher.TranspilerHelper(code, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Extended_CE.Core), nameof(UsingComponents))),
            }, (prev) =>
            {
                CodeInstruction ins = prev.First.Value;
                ins.operand = null;
                ins.opcode = OpCodes.Ldarg_0;
                prev.Clear();
                return new CodeInstruction[]
                {
                    ins,
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MechComponent), "parent")),
                    new CodeInstruction(OpCodes.Isinst, typeof(Mech)),
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CU2ComponentFix), nameof(UsingComponents))),
                };
            });
        }

        private static void FixMech_InitStats_PatchOrder(Harmony h)
        {
            MethodInfo target = AccessTools.Method(typeof(Mech), "InitStats");
            Patches patches = Harmony.GetPatchInfo(target);
            List<Patch> needsReplacement = new List<Patch>();
            foreach (Patch patch in patches.Prefixes)
            {
                if (patch.owner == "BEX.BattleTech.Extended_CE" || patch.owner == "BEX.BattleTech.MechQuirks")
                    needsReplacement.Add(patch);
            }
            foreach (Patch patch in needsReplacement)
            {
                var p = new HarmonyMethod(patch.PatchMethod)
                {
                    before = new string[] { "io.mission.customdeploy" }
                };
                h.Unpatch(target, patch.PatchMethod);
                h.Patch(target, p);
            }
        }
    }
}
