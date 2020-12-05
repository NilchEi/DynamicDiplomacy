using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;


namespace DynamicDiplomacy
{
    class GameCondition_GenerateHistory : GameCondition
    {
        public static int generateHistoryLength;
        public int dynamicExpTick = 0;

        public override void GameConditionTick()
        {
            if (Find.TickManager.TicksGame >= generateHistoryLength)
            {
                Map map = Find.CurrentMap;
                bool flag2 = map.GameConditionManager.ConditionIsActive(GameConditionDefOfLocal.GenerateHistory);
                if (flag2)
                {
                    foreach (GameCondition gameCondition in map.GameConditionManager.ActiveConditions)
                    {
                        bool flag3 = gameCondition.def == GameConditionDefOfLocal.GenerateHistory;
                        if (flag3)
                        {
                            if (Find.TickManager.TicksGame < generateHistoryLength + 2500)
                            {
                                Find.LetterStack.ReceiveLetter("LabelHisGen".Translate(), "DescHisGen".Translate(), LetterDefOf.NeutralEvent, null, null, null, null, null);
                            }
                            map.GameConditionManager.ActiveConditions.Remove(gameCondition);
                            break;
                        }
                    }
                }
            }

            if (Find.TickManager.TicksGame % 30 == 0 && IncidentWorker_NPCConquest.enableConquest)
            {
                doConquest();
            }
            if (Find.TickManager.TicksGame % 100 == 0 && IncidentWorker_NPCDiploChange.enableDiplo)
            {
                doDiploChange();
            }

            if(IncidentWorker_NPCConquest.razeChance != 0)
            {
                dynamicExpTick = (int)(3000 / IncidentWorker_NPCConquest.razeChance);
                if (Find.TickManager.TicksGame % dynamicExpTick == 0 && IncidentWorker_NPCExpansion.enableExpansion)
                {
                    doExpansion();
                }
            }
            else
            {
                if (Find.TickManager.TicksGame % 490 == 0 && IncidentWorker_NPCExpansion.enableExpansion)
                {
                    doExpansion();
                }
            }
        }

        // Conquest
        public static Settlement RandomSettlement()
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where !settlement.Faction.IsPlayer && settlement.Faction.def.settlementGenerationWeight > 0f && !settlement.def.defName.Equals("City_Faction") && !settlement.def.defName.Equals("City_Abandoned") && !settlement.def.defName.Equals("City_Ghost") && !settlement.def.defName.Equals("City_Citadel")
                    select settlement).RandomElementWithFallback(null);
        }

        private static bool HasAnyOtherBase(Settlement defeatedFactionBase)
        {
            List<Settlement> settlements = Find.WorldObjects.Settlements;
            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement settlement = settlements[i];
                if (settlement.Faction == defeatedFactionBase.Faction)
                {
                    return true;
                }
            }
            return false;
        }

        private bool doConquest()
        {
            Settlement AttackerBase = RandomSettlement();
            if (AttackerBase == null)
            {
                return false;
            }
            Faction AttackerFaction = AttackerBase.Faction;
            if (AttackerFaction == null)
            {
                return false;
            }

            List<Settlement> settlements = Find.WorldObjects.Settlements.ToList<Settlement>();
            List<Settlement> prox1 = new List<Settlement>();
            List<Settlement> prox2 = new List<Settlement>();
            List<Settlement> prox3 = new List<Settlement>();
            List<Settlement> prox4 = new List<Settlement>();
            List<Settlement> prox5 = new List<Settlement>();
            List<Settlement> prox6 = new List<Settlement>();
            List<Settlement> prox7 = new List<Settlement>();
            double attackerBaseCount = 0;
            double totalBaseCount = 0;

            Faction rebelFaction = (from x in Find.FactionManager.AllFactionsVisible
                                    where x.def.settlementGenerationWeight > 0f && !x.def.hidden && !x.IsPlayer && !x.defeated
                                    select x).RandomElementWithFallback<Faction>();
            bool rebelCandidateBaseExist = false;
            List<Settlement> attackerSettlementList = new List<Settlement>();

            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement DefenderBase = settlements[i];

                if (!rebelCandidateBaseExist && DefenderBase.Faction == rebelFaction)
                {
                    rebelCandidateBaseExist = true;
                }
                if (DefenderBase.Faction == AttackerBase.Faction)
                {
                    attackerBaseCount++;
                    attackerSettlementList.Add(DefenderBase);
                }

                if (DefenderBase.Faction != null && !DefenderBase.Faction.IsPlayer && DefenderBase.Faction.def.settlementGenerationWeight > 0f && !DefenderBase.def.defName.Equals("City_Faction") && !DefenderBase.def.defName.Equals("City_Abandoned") && !DefenderBase.def.defName.Equals("City_Ghost") && !DefenderBase.def.defName.Equals("City_Citadel"))
                {
                    totalBaseCount++;
                    if (AttackerBase.Faction.HostileTo(DefenderBase.Faction))
                    {
                        int attackDistance = Find.WorldGrid.TraversalDistanceBetween(AttackerBase.Tile, DefenderBase.Tile, true);
                        if (attackDistance < 30)
                        {
                            prox1.Add(DefenderBase);
                        }
                        else if (attackDistance < 60)
                        {
                            prox2.Add(DefenderBase);
                        }
                        else if (attackDistance < 90)
                        {
                            prox3.Add(DefenderBase);
                        }
                        else if (attackDistance < 120)
                        {
                            prox4.Add(DefenderBase);
                        }
                        else if (attackDistance < 150)
                        {
                            prox5.Add(DefenderBase);
                        }
                        else if (attackDistance < 180)
                        {
                            prox6.Add(DefenderBase);
                        }
                        else if (attackDistance < 210)
                        {
                            prox7.Add(DefenderBase);
                        }
                    }
                }
            }

            // Rebellion code
            if (!rebelCandidateBaseExist && (attackerBaseCount >= 20 || attackerBaseCount >= (totalBaseCount * 0.2)) && rebelFaction != null)
            {
                for (int i = 0; i < attackerSettlementList.Count; i++)
                {
                    int num = Rand.Range(1, 100);
                    bool resistancechance = num < 41;
                    if (resistancechance)
                    {
                        Settlement rebelSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        rebelSettlement.SetFaction(rebelFaction);
                        rebelSettlement.Tile = attackerSettlementList[i].Tile;
                        rebelSettlement.Name = SettlementNameGenerator.GenerateSettlementName(rebelSettlement, null);
                        Find.WorldObjects.Remove(attackerSettlementList[i]);
                        Find.WorldObjects.Add(rebelSettlement);
                    }
                }

                FactionRelation factionRelation = rebelFaction.RelationWith(AttackerBase.Faction, false);
                factionRelation.kind = FactionRelationKind.Hostile;
                FactionRelation factionRelation2 = AttackerBase.Faction.RelationWith(rebelFaction, false);
                factionRelation2.kind = FactionRelationKind.Hostile;
                Find.LetterStack.ReceiveLetter("LabelRebellion".Translate(), "DescRebellion".Translate(rebelFaction, AttackerBase.Faction), LetterDefOf.NeutralEvent, null);
                return true;
            }

            // Conquest code
            Settlement FinalDefenderBase;

            if (prox1.Count != 0)
            {
                FinalDefenderBase = prox1.RandomElement<Settlement>();
            }
            else if (prox2.Count != 0)
            {
                FinalDefenderBase = prox2.RandomElement<Settlement>();
            }
            else if (prox3.Count != 0)
            {
                FinalDefenderBase = prox3.RandomElement<Settlement>();
            }
            else if (prox4.Count != 0)
            {
                FinalDefenderBase = prox4.RandomElement<Settlement>();
            }
            else if (prox5.Count != 0)
            {
                FinalDefenderBase = prox5.RandomElement<Settlement>();
            }
            else if (prox6.Count != 0)
            {
                FinalDefenderBase = prox6.RandomElement<Settlement>();
            }
            else if (prox7.Count != 0)
            {
                FinalDefenderBase = prox7.RandomElement<Settlement>();
            }
            else
            {
                return false;
            }

            // Determine whether to raze or take control, distance-based
            int razeroll = Rand.Range(1, 100);
            if (razeroll <= IncidentWorker_NPCConquest.razeChance)
            {
                DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
                destroyedSettlement.Tile = FinalDefenderBase.Tile;
                Find.WorldObjects.Remove(FinalDefenderBase);
                Find.WorldObjects.Add(destroyedSettlement);
                Find.LetterStack.ReceiveLetter("LabelConquestRaze".Translate(), "DescConquestRaze".Translate(FinalDefenderBase.Faction.Name, AttackerBase.Faction.Name), LetterDefOf.NeutralEvent, destroyedSettlement, null, null);
            }
            else
            {
                Settlement settlementConquest = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlementConquest.SetFaction(AttackerBase.Faction);
                settlementConquest.Tile = FinalDefenderBase.Tile;
                settlementConquest.Name = SettlementNameGenerator.GenerateSettlementName(settlementConquest, null);
                Find.WorldObjects.Remove(FinalDefenderBase);
                Find.WorldObjects.Add(settlementConquest);
            }

            // Defeat check for distance conquest
            int defeatroll = Rand.Range(1, 100);
            if (defeatroll <= IncidentWorker_NPCConquest.defeatChance && !HasAnyOtherBase(FinalDefenderBase))
            {
                FinalDefenderBase.Faction.defeated = true;
                Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(FinalDefenderBase.Faction.Name), LetterDefOf.NeutralEvent, null);
            }

            // Alliance code
            if (IncidentWorker_NPCConquest.allowAlliance && Find.World.GetComponent<DiplomacyWorldComponent>().allianceCooldown <= 0)
            {
                List<Faction> alliance = (from x in Find.FactionManager.AllFactionsVisible
                                          where x.def.settlementGenerationWeight > 0f && !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerFaction && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned
                                          select x).ToList<Faction>();
                List<Faction> finalAlliance = new List<Faction>();

                if (alliance.Count >= 2 && attackerBaseCount >= (totalBaseCount * 0.4) && attackerBaseCount <= (totalBaseCount * 0.6) && attackerBaseCount > 9)
                {
                    for (int i = 0; i < alliance.Count; i++)
                    {
                        int num = Rand.Range(1, 100);
                        bool havemysword = num < 81;
                        if (havemysword)
                        {
                            FactionRelation factionRelation = AttackerFaction.RelationWith(alliance[i], false);
                            factionRelation.kind = FactionRelationKind.Hostile;
                            FactionRelation factionRelation2 = alliance[i].RelationWith(AttackerFaction, false);
                            factionRelation2.kind = FactionRelationKind.Hostile;
                            finalAlliance.Add(alliance[i]);
                        }
                    }

                    StringBuilder allianceList = new StringBuilder();
                    for (int x = 0; x < finalAlliance.Count; x++)
                    {
                        for (int y = 0; y < finalAlliance.Count; y++)
                        {
                            if (finalAlliance[y] != finalAlliance[x])
                            {
                                FactionRelation factionRelation3 = finalAlliance[y].RelationWith(finalAlliance[x], false);
                                factionRelation3.kind = FactionRelationKind.Neutral;
                                FactionRelation factionRelation4 = finalAlliance[x].RelationWith(finalAlliance[y], false);
                                factionRelation4.kind = FactionRelationKind.Neutral;
                            }
                        }
                        allianceList.Append(finalAlliance[x].ToString()).Append(", ");
                    }
                    string allianceListString = allianceList.ToString();
                    allianceListString = allianceListString.Trim().TrimEnd(',');

                    Find.LetterStack.ReceiveLetter("LabelAlliance".Translate(), "DescAlliance".Translate(allianceListString, AttackerBase.Faction), LetterDefOf.NeutralEvent, null);
                    Find.World.GetComponent<DiplomacyWorldComponent>().allianceCooldown = 11;
                }
            }

            if (Find.World.GetComponent<DiplomacyWorldComponent>().allianceCooldown > 0)
            {
                Find.World.GetComponent<DiplomacyWorldComponent>().allianceCooldown--;
            }

            return true;
        }

        //Diplo change
        private bool TryFindFaction(bool allowPerm, bool excludeEmpire, out Faction faction)
        {
            if (allowPerm)
            {
                if (excludeEmpire)
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

        private bool doDiploChange()
        {
            Faction faction;
            Faction faction2;

            if (!this.TryFindFaction(IncidentWorker_NPCDiploChange.allowPerm, IncidentWorker_NPCDiploChange.excludeEmpire, out faction))
            {
                return false;
            }
            if (!this.TryFindFaction2(IncidentWorker_NPCDiploChange.allowPerm, IncidentWorker_NPCDiploChange.excludeEmpire, faction, out faction2))
            {
                return false;
            }

            if (faction.HostileTo(faction2))
            {
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Neutral;
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Neutral;
            }
            else
            {
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Hostile;
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Hostile;
            }

            return true;
        }

        private bool doExpansion()
        {
            int tile = TileFinder.RandomStartingTile();
            if (!TileFinder.IsValidTileForNewSettlement(tile))
            {
                return false;
            }
            List<Settlement> settlements = Find.WorldObjects.Settlements.ToList<Settlement>();
            List<Settlement> candidateSettlements = new List<Settlement>();
            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement SettlerCandidateBase = settlements[i];
                if (SettlerCandidateBase.Faction.IsPlayer || SettlerCandidateBase.Faction.def.settlementGenerationWeight == 0f)
                {
                }
                else
                {
                    if (Find.WorldGrid.TraversalDistanceBetween(tile, SettlerCandidateBase.Tile, true) <= IncidentWorker_NPCExpansion.expansionRadius)
                    {
                        candidateSettlements.Add(SettlerCandidateBase);
                    }
                }
            }

            Settlement SettlerBase;
            if (candidateSettlements.Count != 0)
            {
                SettlerBase = candidateSettlements.RandomElement<Settlement>();
            }
            else
            {
                // fail due to no valid candidate with supply line
                return false;
            }

            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(SettlerBase.Faction);
            bool flag3 = settlement.Faction == null;
            if (flag3)
            {
                return false;
            }
            else
            {
                settlement.Tile = tile;
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
                Find.WorldObjects.Add(settlement);
                Find.LetterStack.ReceiveLetter("LabelExpansion".Translate(), "DescExpansion".Translate(SettlerBase.Faction.Name, SettlerBase.Name, settlement.Name), LetterDefOf.NeutralEvent, settlement, null, null);
            }

            return true;
        }
    }
}
