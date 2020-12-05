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
            return base.CanFireNowSub(parms) && enableDiplo;
        }

        private bool TryFindFaction(bool allowPerm, bool excludeEmpire, out Faction faction)
        {
            if(allowPerm)
            {
                if(excludeEmpire)
                {
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x.def != FactionDefOf.Empire
                            select x).TryRandomElement(out faction);
                }
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction);
            }
            else
            {
                if (excludeEmpire)
                {
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x.def != FactionDefOf.Empire
                            select x).TryRandomElement(out faction);
                }
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction);
            }
        }
        private bool TryFindFaction2(bool allowPerm, bool excludeEmpire, Faction faction, out Faction faction2)
        {
            if (allowPerm)
            {
                if (excludeEmpire)
                {
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && x.def != FactionDefOf.Empire
                            select x).TryRandomElement(out faction2);
                }
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction
                        select x).TryRandomElement(out faction2);
            }
            else
            {
                if (excludeEmpire)
                {
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && x.def != FactionDefOf.Empire
                            select x).TryRandomElement(out faction2);
                }
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction
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

            if (!this.TryFindFaction(allowPerm, excludeEmpire, out faction))
            {
                return false;
            }
            if (!this.TryFindFaction2(allowPerm, excludeEmpire, faction, out faction2))
            {
                return false;
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