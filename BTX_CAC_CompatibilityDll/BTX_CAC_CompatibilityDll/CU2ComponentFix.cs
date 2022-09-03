using AccessExtension;
using BattleTech;
using CustomUnits;
using Harmony;
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
            if (m is CustomMech cm)
            {
                if (cm.isSquad || cm.isVehicle)
                    return false;
            }
            return Extended_CE.Core.UsingComponents();
        }

        public static void Patch(HarmonyInstance h)
        {
            Assembly a = AccessExtensionPatcher.GetLoadedAssemblyByName("Extended_CE");
            if (a == null)
            {
                FileLog.Log("a null");
                return;
            }
            Type t = a.GetType("Extended_CE.BTComponents+Mech_InitStats");
            if (t == null)
            {
                FileLog.Log("t null");
                foreach (var tt in a.GetTypes())
                    FileLog.Log(tt.FullName);
                return;
            }
            var m = AccessTools.DeclaredMethod(t, "Prefix");
            if (m == null)
            {
                FileLog.Log("m null");
                return;
            }
            h.Patch(m, null, null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(CU2ComponentFix), "Transpiler_MechInitStatsPre")));
        }

        public static IEnumerable<CodeInstruction> Transpiler_MechInitStatsPre(IEnumerable<CodeInstruction> code)
        {
            return AccessExtensionPatcher.TranspilerHelper(code, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Extended_CE.Core), "UsingComponents")),
            }, (prev) =>
            {
                prev.Clear();
                return new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CU2ComponentFix), "UsingComponents")),
                };
            });
        }
    }
}
