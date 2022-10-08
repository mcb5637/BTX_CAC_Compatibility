using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.Framework;
using System.Reflection.Emit;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(ContractOverride), "FromJSONFull")]
    class ContractOverride_FromJSONFull
    {
        public static void Postfix(ContractOverride __instance)
        {
            PatchContract(__instance);
        }

        internal static void PatchContract(ContractOverride __instance)
        {
            if (__instance.maxNumberOfPlayerUnits == 4 && !Main.Sett.Use4LimitOnContractIds.Contains(__instance.ID))
            {
                __instance.maxNumberOfPlayerUnits = 8;
                if (__instance.mechMaxTonnages != null && __instance.mechMaxTonnages.Length == 4)
                {
                    __instance.mechMaxTonnages = FixArray(__instance.mechMaxTonnages);
                }
                if (__instance.mechMinTonnages != null && __instance.mechMinTonnages.Length == 4)
                {
                    __instance.mechMinTonnages = FixArray(__instance.mechMinTonnages);
                }
            }
        }

        private static float[] FixArray(float[] old)
        {
            float[] ne = new float[8];
            for (int i = 0; i < 8; i++)
            {
                if (i < old.Length)
                    ne[i] = old[i];
                else
                    ne[i] = old[old.Length - 1];
            }
            return ne;
        }
    }


    [HarmonyPatch(typeof(ContractOverride), "FullRehydrate")]
    class ContractOverride_FullRehydrate
    {
        public static void Postfix(ContractOverride __instance)
        {
            ContractOverride_FromJSONFull.PatchContract(__instance);
        }
    }

    [HarmonyPatch(typeof(ContractOverride), "Copy")]
    class ContractOverride_Copy
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> m)
        {
            foreach (CodeInstruction i in m)
            {
                yield return i;
                if (i.opcode==OpCodes.Newobj)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, typeof(ContractOverride_Copy).GetMethod("AfterCreate"));
                }
            }
        }

        public static ContractOverride AfterCreate(ContractOverride copy, ContractOverride old)
        {
            if (old.mechMaxTonnages != null && old.mechMaxTonnages.Length > 4)
            {
                copy.mechMaxTonnages = new float[old.mechMaxTonnages.Length];
            }
            if (old.mechMinTonnages != null && old.mechMinTonnages.Length > 4)
            {
                copy.mechMinTonnages = new float[old.mechMinTonnages.Length];
            }
            return copy;
        }
    }
}
