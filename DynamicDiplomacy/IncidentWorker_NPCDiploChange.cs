using System.Linq;
using RimWorld;
using Verse;

namespace DynamicDiplomacy
{
    public class IncidentWorker_NPCDiploChange : IncidentWorker
    {
        public static bool allowPerm;
        public static bool enableDiplo;
        public static bool excludeEmpire;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction faction;
            Faction faction2;
            return base.CanFireNowSub(parms) && this.TryFindFaction(allowPerm, out faction) && this.TryFindFaction2(allowPerm, out faction2) && enableDiplo;
        }

        private bool TryFindFaction(bool allowPerm, out Faction faction)
        {
            if(allowPerm)
            {
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction);
            }
            else
            {
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction);
            }
        }
        private bool TryFindFaction2(bool allowPerm, out Faction faction2)
        {
            if (allowPerm)
            {
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction2);
            }
            else
            {
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction2);
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!enableDiplo)
            {
                return false;
            }

            Faction faction;
            Faction faction2;
            int reroll = 0;

            if (!this.TryFindFaction(allowPerm, out faction))
            {
                return false;
            }
            if (!this.TryFindFaction2(allowPerm, out faction2))
            {
                return false;
            }

            // exclude same factions
            while (faction == faction2)
            {
                Log.Message("same faction selected, reroll second faction");
                this.TryFindFaction2(allowPerm, out faction2);
                if (reroll > 10)
                {
                    Log.Message("Ten unsuccessful rerolls. Exit.");
                    return false;
                }
                reroll++;
            }

            // exclude Empire depending on setting
            if (excludeEmpire && (faction.def == FactionDefOf.Empire || faction2.def == FactionDefOf.Empire))
            {
                while (faction.def == FactionDefOf.Empire || faction2.def == FactionDefOf.Empire)
                {                    
                    Log.Message("Empire disallowed, reroll another faction");
                    this.TryFindFaction(allowPerm, out faction);
                    this.TryFindFaction2(allowPerm, out faction2);
                    if (reroll > 10)
                    {
                        Log.Message("Ten unsuccessful rerolls. Exit.");
                        return false;
                    }
                    reroll++;
                }
            }

            if (faction.HostileTo(faction2))
            {
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Neutral;
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Neutral;
                Find.LetterStack.ReceiveLetter("LabelDCPeace".Translate(), "DescDCPeace".Translate(faction.Name, faction2.Name), LetterDefOf.NeutralEvent, null);
            }
            else
            {
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Hostile;
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Hostile;
                Find.LetterStack.ReceiveLetter("LabelDCWar".Translate(), "DescDCWar".Translate(faction.Name, faction2.Name), LetterDefOf.NeutralEvent, null);
            }

            return true;
        }
    }
}