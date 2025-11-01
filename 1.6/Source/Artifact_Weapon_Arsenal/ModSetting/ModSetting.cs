using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Artifact_Weapon_Arsenal
{
    public class AWAModSetting : ModSettings
    {
        public static float MeleeDamage = 1f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref MeleeDamage, "MeleeDamage", 1f);
        }
        public void InitData()
        {
            MeleeDamage = 1f;
        }
    }
}