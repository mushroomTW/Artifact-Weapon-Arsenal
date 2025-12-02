using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Artifact_Weapon_Arsenal
{
    public class InstantDead : DamageWorker
    {
        // 當這個傷害類型被應用時，這個函式會被呼叫
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            DamageResult damageResult = new DamageResult();

            // 檢查目標是否為 Pawn (角色)
            if (thing is Pawn pawn)
            {
                // ❗ 核心：直接呼叫 Kill 函式，無視血量
                pawn.Kill(dinfo);
            }
            else
            {
                // 如果不是角色 (例如建築物、物品)，則直接銷毀
                thing.Destroy(DestroyMode.KillFinalize);
            }

            return damageResult;
        }
    }
}
