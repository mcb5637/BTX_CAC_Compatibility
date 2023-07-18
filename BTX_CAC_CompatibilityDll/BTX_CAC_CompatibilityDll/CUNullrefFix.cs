using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BattleTech;
using CustomUnits;
using System.Reflection.Emit;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Pathing_UpdateMeleePath), "Postfix")]
    class CUNullrefFix
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            foreach (CodeInstruction c in instructions)
            {
                //if (c.opcode == OpCodes.Ldstr && "Pathing.UpdateMeleePath ".Equals(c.operand))
                //    c.operand = "Pathing.UpdateMeleePath Modified ";
                if (c.opcode == OpCodes.Brfalse && found)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    c.opcode = OpCodes.Br;
                    Main.Log.Log("fixed CU update pathing nullref");
                }

                if (c.opcode == OpCodes.Call && "Void PrependStoredPath(BattleTech.Pathing, System.Collections.Generic.List`1[BattleTech.PathNode] ByRef)".Equals(c.operand.ToString()))
                    found = true;

                //FileLog.Log($"{c.opcode} {(c.operand == null ? "null" : c.operand.ToString())} {(c.operand == null ? "null" : c.operand.GetType().FullName)}");
                yield return c;
            }
        }
    }
}
