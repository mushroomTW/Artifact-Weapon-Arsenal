using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Artifact_Weapon_Arsenal
{
    public class LightningStrike : DamageWorker
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            // 1. 先執行正常的傷害計算 (讓武器還是會砍傷人)
            DamageResult damageResult = base.Apply(dinfo, victim);

            // 2. 獲取攻擊者 (Instigator)
            Pawn attacker = dinfo.Instigator as Pawn;

            // 3. 🛡️ 賦予無敵 Buff
            if (attacker != null)
            {
                // 獲取我們在 XML 定義的 Hediff
                HediffDef protectionDef = HediffDef.Named("AWA_LightningProtection");

                // 賦予 Buff
                 attacker.health.AddHediff(protectionDef);    
            }

            // 4. ⚡ 觸發雷擊 (現在攻擊者無敵了，炸也不怕)
            if (victim != null && victim.Map != null)
            {
                // 創建一個雷擊事件，位置在受害者腳下
                // 這會造成：閃電視覺效果 + 雷聲 + 爆炸傷害 + 點火
                victim.Map.weatherManager.eventHandler.AddEvent(
                    new WeatherEvent_LightningStrike(victim.Map, victim.Position)
                );
            }

            return damageResult;
        }
    }
}
