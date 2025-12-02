using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Artifact_Weapon_Arsenal
{
    public class Ture_Flame : DamageWorker_AddInjury
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            Pawn pawn = victim as Pawn;
            Map map = victim.Map;
            DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);
            if (map == null)
                return damageResult;
            if (!damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(victim)))
                victim.TryAttachFire(1.75f, dinfo.Instigator);
            if (victim.Destroyed && pawn == null)
            {
                foreach (IntVec3 c in victim.OccupiedRect())
                    FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Ash);
            }
            return damageResult;
        }

        public override void ExplosionAffectCell(
          Explosion explosion,
          IntVec3 c,
          List<Thing> damagedThings,
          List<Thing> ignoredThings,
          bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            if (this.def != DamageDefOf.Flame || !Rand.Chance(FireUtility.ChanceToStartFireIn(c, explosion.Map)))
                return;
            FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f), explosion.instigator);
        }
    }
}
