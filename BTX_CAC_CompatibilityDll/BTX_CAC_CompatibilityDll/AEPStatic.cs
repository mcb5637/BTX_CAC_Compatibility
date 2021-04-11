using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AccessExtension;

namespace BTX_CAC_CompatibilityDll
{
    static class AEPStatic
    {
        [PropertyGet(typeof(SequenceBase), "Combat")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static CombatGameState GetCombat(this SequenceBase s)
        {
            return null;
        }
    }
}
