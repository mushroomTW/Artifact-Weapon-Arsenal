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

    public class CompMoreAbility : CompEquippable
    {
        public int Killcount = 0;
        public int TickToCheck;
        protected bool biocoded;
        protected Pawn codedPawn;
        protected string codedPawnLabel;

        public CompProperties_MoreAbility Props => this.props as CompProperties_MoreAbility;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (this.Holder == null)
                return;
            foreach (Ability ability in this.AbilitysForReading)
            {
                ability.pawn = this.Holder;
                ability.verb.caster = (Thing)this.Holder;
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            foreach (Ability ability in this.AbilitysForReading)
            {
                ability.pawn = pawn;
                ability.verb.caster = (Thing)pawn;
                pawn.abilities.GainAbility(ability.def);
            }
            this.CodeFor(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            foreach (Ability ability in this.AbilitysForReading)
                pawn.abilities.RemoveAbility(ability.def);
        }

        public void CodeFor(Pawn pawn)
        {
            if (!this.Biocodable)
                return;
            this.biocoded = true;
            this.codedPawn = pawn;
            this.codedPawnLabel = pawn.Name.ToStringFull;
            this.OnCodedFor(pawn);
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            ++this.Killcount;
            Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
            if (psychicEntropy == null || !this.Props.GivePE)
                return;
            psychicEntropy.OffsetPsyfocusDirectly(Mathf.Max(0.5f, 0.07f * (float)pawn.GetPsylinkLevel()));
        }

        public void UnCode()
        {
            this.biocoded = false;
            Pawn codedPawn = this.CodedPawn;
            this.codedPawn = (Pawn)null;
            this.codedPawnLabel = (string)null;
            this.Killcount = 0;
        }

        public List<Ability> AbilitysForReading
        {
            get
            {
                List<Ability> abilitysForReading = new List<Ability>();
                foreach (AbilityDef abilitieDef in this.Props.AbilitieDefs)
                    abilitysForReading.Add(AbilityUtility.MakeAbility(abilitieDef, this.Holder));
                Log.Message("ability" + abilitysForReading?.ToString());
                return abilitysForReading;
            }
        }

        public void ExposeData() => Scribe_Values.Look<int>(ref this.Killcount, "no. of kills");

        public virtual bool Biocodable => true;

        public Pawn CodedPawn => this.codedPawn;

        protected virtual void OnCodedFor(Pawn p)
        {
        }
    }

    public class CompProperties_MoreAbility : CompProperties
    {
        public bool biocodeOnEquip = true;
        public List<AbilityDef> AbilitieDefs;
        public bool GivePE = true;

        public CompProperties_MoreAbility() => this.compClass = typeof(CompMoreAbility);
    }


}
