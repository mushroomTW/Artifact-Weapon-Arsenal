using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Artifact_Weapon_Arsenal
{
    [StaticConstructorOnStartup]
    [DefOf]
    public class AWA_Def
    {

        public static ThingDef Mote_ChipBoosted;
        static AWA_Def() => DefOfHelper.EnsureInitializedInCtor(typeof(AWA_Def));
    }

}
