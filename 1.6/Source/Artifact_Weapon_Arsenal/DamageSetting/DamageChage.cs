using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artifact_Weapon_Arsenal
{
    public class DamageChage
    {
        public static float GetMeleeDamage(float damage)
        {
            return damage * AWAModSetting.MeleeDamage;
        }
    }
}
