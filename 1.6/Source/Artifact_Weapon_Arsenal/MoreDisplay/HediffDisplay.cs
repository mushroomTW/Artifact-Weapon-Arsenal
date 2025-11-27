using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Artifact_Weapon_Arsenal
{
    public class HediffComp_HediffDisplay : HediffComp
    {
        public Mote Mote;

        public HediffCompProperties_HediffDisplay Props
        {
            get => (HediffCompProperties_HediffDisplay)this.props;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look<Mote>(ref this.Mote, "Mote");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.parent.pawn;
            base.CompPostTick(ref severityAdjustment);
            bool flag = !pawn.InBed() && pawn.Awake() && !pawn.Downed;
            if (this.Mote.DestroyedOrNull())
            {
                this.Mote = MoteMaker.MakeAttachedOverlay((Thing)this.parent.pawn, AWA_Def.Mote_ChipBoosted, new Vector3(0.0f, 0.0f, -0.05f), 2.3f, 1f);
                this.Mote.exactRotation = 0.0f;
            }
            if (!flag)
                return;
            this.Mote.Maintain();
        }
    }
    public class HediffCompProperties_HediffDisplay : HediffCompProperties
    {
        public float R = (float)byte.MaxValue;
        public float G = (float)byte.MaxValue;
        public float B = (float)byte.MaxValue;
        public float yOffset = 0.0f;

        public HediffCompProperties_HediffDisplay() => this.compClass = typeof(HediffComp_HediffDisplay);
    }
}
