﻿using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AccessExtension;
using System.Reflection;
using Extended_CE;

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

        [FieldGet("Extended_CE", "Extended_CE.BTComponents", "CheckingForCrit")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool GetExtendedCE_BtComponents_CheckingForCrit()
        {
            return false;
        }

        [PropertyGet(typeof(Pathing), "SprintingGrid")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PathNodeGrid SprintingGrid(this Pathing s)
        {
            return null;
        }

        [PropertyGet(typeof(Pathing), "WalkingGrid")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PathNodeGrid WalkingGrid(this Pathing s)
        {
            return null;
        }

        [PropertyGet("Extended_CE", "Extended_CE.BTComponents", "Actuators")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ActuatorInfo GetActuatorInfo()
        {
            return null;
        }

        [FieldSet(typeof(MechDef), "inventory")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetInvNoCheck(this MechDef d, MechComponentRef[] i)
        {

        }
        [FieldSet(typeof(WorkOrderEntry_MechLab), "CBillCost")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetCBillCost(this WorkOrderEntry_MechLab d, int i)
        {

        }
    }
}
