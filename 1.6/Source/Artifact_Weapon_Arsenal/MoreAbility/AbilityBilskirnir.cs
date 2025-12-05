using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Artifact_Weapon_Arsenal
{
    public class CompAbilityEffect_Bilskirnir : CompAbilityEffect
    {
        private HashSet<Faction> affectedFactionCache = new HashSet<Faction>();

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            // 1. 生成視覺中心點物件 (如果需要)
            // 這裡使用原版的 Flashstorm 物件作為標記，或者你可以定義自己的
            //Thing thing = GenSpawn.Spawn(ThingDefOf.Flashstorm, target.Cell, this.parent.pawn.Map);

            // 2. ❗ 獲取你自定義的 GameConditionDef
            // 使用 DefDatabase.GetNamed 獲取你在 XML 中定義的 "BilskirnirStorm"
            GameConditionDef myConditionDef = DefDatabase<GameConditionDef>.GetNamed("Bilskirnir");

            // 3. ❗ 生成 Condition (這會建立 GameCondition_Bilskirnir 的實例)
            GameCondition_Bilskirnir cond = (GameCondition_Bilskirnir)GameConditionMaker.MakeCondition(myConditionDef);

            cond.centerLocation = target.Cell.ToIntVec2;
            cond.areaRadiusOverride = new IntRange(Mathf.RoundToInt(this.parent.def.EffectRadius), Mathf.RoundToInt(this.parent.def.EffectRadius));
            cond.Duration = Mathf.RoundToInt((float)this.parent.def.EffectDuration(this.parent.pawn).SecondsToTicks());
            cond.suppressEndMessage = true;
            cond.initialStrikeDelay = new IntRange(0, 60);
            cond.conditionCauser = this.parent.pawn;
            cond.ambientSound = true;
            map.gameConditionManager.RegisterCondition((GameCondition)cond);
            this.ApplyGoodwillImpact(target, cond.AreaRadius);
        }

        private void ApplyGoodwillImpact(LocalTargetInfo target, int radius)
        {
            if (this.parent.pawn.Faction != Faction.OfPlayer)
                return;
            this.affectedFactionCache.Clear();
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(target.Cell, this.parent.pawn.Map, (float)radius, true))
            {
                if (thing is Pawn p && thing.Faction != null && thing.Faction != this.parent.pawn.Faction && !thing.Faction.HostileTo(this.parent.pawn.Faction) && !this.affectedFactionCache.Contains(thing.Faction) && (this.Props.applyGoodwillImpactToLodgers || !p.IsQuestLodger()))
                {
                    this.affectedFactionCache.Add(thing.Faction);
                    Faction.OfPlayer.TryAffectGoodwillWith(thing.Faction, this.Props.goodwillImpact, reason: HistoryEventDefOf.UsedHarmfulAbility);
                }
            }
            this.affectedFactionCache.Clear();
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!target.Cell.Roofed(this.parent.pawn.Map))
                return true;
            if (throwMessages)
                Messages.Message((string)("CannotUseAbility".Translate((NamedArgument)this.parent.def.label) + ": " + "AbilityRoofed".Translate()), (LookTargets)target.ToTargetInfo(this.parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
            return false;
        }
    }

    public class CompProperties_AbilityBilskirnir : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityBilskirnir()
        {
            this.compClass = typeof(CompAbilityEffect_Bilskirnir);
        }
    }
    public class GameCondition_Bilskirnir : GameCondition
    {
        //如果沒有指定範圍（例如隨機事件），風暴半徑會在 45 到 60 格之間隨機決定
        private static readonly IntRange AreaRadiusRange = new IntRange(45, 60);
        //落雷的頻率
        private static readonly IntRange TicksBetweenStrikes = new IntRange(80, 160);
        private const int RainDisableTicksAfterConditionEnds = 30000;
        private const int AvoidConditionCauserExpandRect = 2;
        public IntVec2 centerLocation = IntVec2.Invalid;
        public IntRange areaRadiusOverride = IntRange.Zero;
        public IntRange initialStrikeDelay = IntRange.Zero;
        public bool ambientSound;
        private int areaRadius;
        private int nextLightningTicks;
        private Sustainer soundSustainer;
        public bool avoidConditionCauser;

        public int AreaRadius => this.areaRadius;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec2>(ref this.centerLocation, "centerLocation");
            Scribe_Values.Look<int>(ref this.areaRadius, "areaRadius");
            Scribe_Values.Look<IntRange>(ref this.areaRadiusOverride, "areaRadiusOverride");
            Scribe_Values.Look<int>(ref this.nextLightningTicks, "nextLightningTicks");
            Scribe_Values.Look<IntRange>(ref this.initialStrikeDelay, "initialStrikeDelay");
            Scribe_Values.Look<bool>(ref this.ambientSound, "ambientSound");
            Scribe_Values.Look<bool>(ref this.avoidConditionCauser, "avoidConditionCauser");
        }

        public override void Init()
        {
            base.Init();
            this.areaRadius = this.areaRadiusOverride == IntRange.Zero ? GameCondition_Bilskirnir.AreaRadiusRange.RandomInRange : this.areaRadiusOverride.RandomInRange;
            this.nextLightningTicks = Find.TickManager.TicksGame + this.initialStrikeDelay.RandomInRange;
            if (!this.centerLocation.IsInvalid)
                return;
            this.FindGoodCenterLocation();
        }

        public override void GameConditionTick()
        {
            if (Find.TickManager.TicksGame > this.nextLightningTicks)
            {
                Vector2 vector2 = Rand.UnitVector2 * Rand.Range(0.0f, (float)this.areaRadius);
                IntVec3 intVec3 = new IntVec3((int)Math.Round((double)vector2.x) + this.centerLocation.x, 0, (int)Math.Round((double)vector2.y) + this.centerLocation.z);
                if (this.IsGoodLocationForStrike(intVec3))
                {
                    this.SingleMap.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrike(this.SingleMap, intVec3));
                    this.nextLightningTicks = Find.TickManager.TicksGame + GameCondition_Bilskirnir.TicksBetweenStrikes.RandomInRange;
                }
            }
            if (!this.ambientSound)
                return;
            if (this.soundSustainer == null || this.soundSustainer.Ended)
                this.soundSustainer = SoundDefOf.FlashstormAmbience.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(this.centerLocation.ToIntVec3, this.SingleMap), MaintenanceType.PerTick));
            else
                this.soundSustainer.Maintain();
        }

        public override void End()
        {
            this.SingleMap.weatherDecider.DisableRainFor(30000);
            base.End();
        }

        private void FindGoodCenterLocation()
        {
            if (this.SingleMap.Size.x <= 16 /*0x10*/ || this.SingleMap.Size.z <= 16 /*0x10*/)
                throw new Exception("Map too small for flashstorm.");
            for (int index = 0; index < 10; ++index)
            {
                this.centerLocation = new IntVec2(Rand.Range(8, this.SingleMap.Size.x - 8), Rand.Range(8, this.SingleMap.Size.z - 8));
                if (this.IsGoodCenterLocation(this.centerLocation))
                    break;
            }
        }

        private bool IsGoodLocationForStrike(IntVec3 loc)
        {
            if (!loc.InBounds(this.SingleMap) || loc.Roofed(this.SingleMap) || !loc.Standable(this.SingleMap))
                return false;
            if (this.avoidConditionCauser && this.conditionCauser != null)
            {
                CellRect cellRect = this.conditionCauser.OccupiedRect();
                cellRect = cellRect.ExpandedBy(2);
                if (cellRect.Contains(loc))
                    return false;
            }
            return true;
        }

        private bool IsGoodCenterLocation(IntVec2 loc)
        {
            int num1 = 0;
            int num2 = (int)(3.1415927410125732 * (double)this.areaRadius * (double)this.areaRadius / 2.0);
            foreach (IntVec3 potentiallyAffectedCell in this.GetPotentiallyAffectedCells(loc))
            {
                if (this.IsGoodLocationForStrike(potentiallyAffectedCell))
                    ++num1;
                if (num1 >= num2)
                    break;
            }
            return num1 >= num2;
        }

        private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
        {
            for (int x = center.x - this.areaRadius; x <= center.x + this.areaRadius; ++x)
            {
                for (int z = center.z - this.areaRadius; z <= center.z + this.areaRadius; ++z)
                {
                    if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= this.areaRadius * this.areaRadius)
                        yield return new IntVec3(x, 0, z);
                }
            }
        }
    }

}
