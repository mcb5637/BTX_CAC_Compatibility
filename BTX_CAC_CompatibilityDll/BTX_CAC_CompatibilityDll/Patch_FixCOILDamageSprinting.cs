using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Weapon), "GetCOILDamageFromEvasivePips")]
    class Weapon_GetCOILDamageFromEvasivePips
    {
        public static void Prefix(ref bool parentSprinted)
        {
            parentSprinted = false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "RefreshDisplayedWeapon")]
    class CombatHUDWeaponSlot_RefreshDisplayedWeapon
    {
        public static SelectionType ModSelType(SelectionType t)
        {
            if (t == SelectionType.Sprint || t == SelectionType.Melee || t == SelectionType.DFA || t == SelectionType.Backward || t == SelectionType.SprintNonInterleavedDEPRECATED)
                t = SelectionType.Move;
            return t;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            bool seltype = false;
            foreach (CodeInstruction c in code)
            {
                if (seltype && c.opcode == OpCodes.Ldc_I4_3) // sprint
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_6); // jump
                    seltype = false;
                    continue;
                }
                if (seltype && c.opcode == OpCodes.Ldc_I4_1) // move
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon).GetMethod("ModSelType"));
                }

                seltype = c.opcode == OpCodes.Callvirt && c.operand.ToString().Equals("BattleTech.UI.SelectionType get_SelectionType()");
                yield return c;
            }
        }
    }
}
