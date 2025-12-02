using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Artifact_Weapon_Arsenal
{
    public class CompAbilityEffect_Thunderbolt : CompAbilityEffect
    {
        public new CompProperties_AbilityThunderbolt Props => (CompProperties_AbilityThunderbolt)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Map map = this.parent.pawn.Map;

            for (int i = 0; i < Props.strikeCount; i++)
            {
                int currentDelay = i * Props.ticksBetweenStrikes;

                Delay.AfterNTicks(currentDelay, () =>
                {
                    // 1. 基礎安全檢查
                    if (map == null || map.weatherManager == null) return;

                    IntVec3 currentCenter;

                    // 2. 針對 Pawn (生物) 的特殊檢查
                    if (target.HasThing && target.Thing is Pawn p)
                    {
                        // 💀 核心修改：如果小人死了、被銷毀了、或不在地圖上了 -> 停止雷擊
                        if (p.Dead || p.Destroyed || !p.Spawned || p.Map != map)
                        {
                            return; // 直接結束，這道雷不會劈下來
                        }

                        // 如果還活著，追蹤當前位置
                        currentCenter = p.Position;
                    }
                    else if (target.HasThing && target.Thing.Spawned)
                    {
                        // 如果是建築物或物品，追蹤位置
                        currentCenter = target.Thing.Position;
                    }
                    else
                    {
                        // 如果是地板或目標已消失的非生物，打在原地
                        currentCenter = target.Cell;
                    }

                    IntVec3 strikeLoc;

                    // 3. 計算落點
                    if (Props.radius <= 0)
                    {
                        strikeLoc = currentCenter;
                    }
                    else
                    {
                        strikeLoc = CellRect.CenteredOn(currentCenter, (int)Props.radius).RandomCell;
                    }

                    // 4. 生成雷擊
                    if (strikeLoc.InBounds(map))
                    {
                        map.weatherManager.eventHandler.AddEvent(
                            new WeatherEvent_LightningStrike(map, strikeLoc)
                        );
                    }
                });
            }
        }
    }
    

    public class CompProperties_AbilityThunderbolt : CompProperties_AbilityEffect
    {
        public int strikeCount = 3;        // 雷擊總數 (預設 3 道)
        public float radius = 0f;        // 雷擊散佈半徑
        public int ticksBetweenStrikes = 120; // 每道雷擊的間隔時間 (Ticks)
        public CompProperties_AbilityThunderbolt()
        {
            this.compClass = typeof(CompAbilityEffect_Thunderbolt);
        }
    }

}
