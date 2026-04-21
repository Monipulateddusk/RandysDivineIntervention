using UnityEngine;

namespace TurnBased.Phases {

    public class CombatTurnOrchestrator
    {
        private readonly PhaseManager phaseManager = new();
        private readonly SubPhaseManager subPhaseManager = new();
        private readonly Intention.CombatRoundUnitIntentionManager combatRoundIntentionManager;

        private System.Collections.Generic.List<UnitIndex> currentUnitsToProcessIntentions = new();

        private static CombatTurnOrchestrationPhase currentOrchestrationPhase = CombatTurnOrchestrationPhase.StartOfBattle;
        public static CombatTurnOrchestrationPhase CurrentOrchestrationPhase
        {
            get
            {
                return currentOrchestrationPhase;
            }
            set
            {
                /*  Safe guard to not do the start of battle multiple times per encounter.  */
                if (value != CombatTurnOrchestrationPhase.StartOfBattle)
                {
                    currentOrchestrationPhase = value;
                }
            }
        }

        public CombatTurnOrchestrator()
        {
            this.combatRoundIntentionManager = new(this);
        }

        public void Awake()
        {
            this.phaseManager.Awake(OnPhaseComplete);
            this.subPhaseManager.Awake(OnSubPhaseComplete);
        }

        public void Update()
        {
            this.phaseManager.Update();
            this.subPhaseManager.Update();


            MonoBehaviour.print($"<color=black>Current main phase state is: {currentOrchestrationPhase.ToString()}.</color> " +
                $"<color=white> Current sub phase: {this.subPhaseManager.CurrentSubPhaseState.ToString()} </color>");

        }

        public void Start()
        {
            this.phaseManager.ChangeState(CombatTurnOrchestrationPhase.StartOfBattle);
        }

        public void ChangeSubPhase(SubPhaseState subPhaseToGoInto, UnitIndex unitResolvingSubPhase)
        {
            this.subPhaseManager.SwitchSubPhase(subPhaseToGoInto, unitResolvingSubPhase);
        }


        private void OnPhaseComplete(CombatTurnOrchestrationPhase phaseThatCompleted)
        {
            StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
            /*  Try and get the Unit index on that station to pass to the subphase we are entering. */
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndexOnStation)) { return; }


            switch (phaseThatCompleted)
            {
                case CombatTurnOrchestrationPhase.StartOfBattle:

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.StartOfRound;
                    this.phaseManager.ChangeState(CurrentOrchestrationPhase);
                    break;


                case CombatTurnOrchestrationPhase.StartOfRound:

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.PrePlayerTurn;
                    this.phaseManager.ChangeState(CurrentOrchestrationPhase);
                    break;


                case CombatTurnOrchestrationPhase.PrePlayerTurn:

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.PlayerTurn;
                    this.phaseManager.ChangeState(CurrentOrchestrationPhase);
                    break;


                case CombatTurnOrchestrationPhase.PlayerTurn:

                    /*  
                     *  IMPORTANT: THIS CONNECTION NEEDS TO BE REVISED. WE SHOULD NOT BE GOING INTO A SUBPHASE FROM WITHIN A PHASE.
                     *  WE SHOULD BE TOLD TO BY THIS CLASS!!!
                     */
                    this.subPhaseManager.SwitchSubPhase(SubPhaseState.RESOLVE_ATTACK, unitIndexOnStation);


                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.EndOfRound;
                    this.phaseManager.ChangeState(CurrentOrchestrationPhase);
                    break;


                case CombatTurnOrchestrationPhase.EndOfRound:

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.StartOfRound;
                    this.phaseManager.ChangeState(CurrentOrchestrationPhase);
                    break;


                default:
                    break;
            }
        }



        private void OnSubPhaseComplete(SubPhaseState phaseThatCompleted)
        {
            /*  Retrieve the selected station.  */
            StationIndex selectedStation = StationSelectorManager.Instance.GetSelectedStationIndex();

            /*  Try and get the Unit index on that station to pass to the subphase we are entering. */
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStation, out UnitIndex unitIndexOnStation)) { return; }

            switch (phaseThatCompleted)
            {
                case SubPhaseState.AWAITING_MOVE_SELECTION:

                    this.combatRoundIntentionManager.CompleteAwaitingUserInput(unitIndexOnStation);

                    this.subPhaseManager.SwitchSubPhase(SubPhaseState.AWAITING_TARGET_SELECTION, unitIndexOnStation);
                    break;
                case SubPhaseState.AWAITING_TARGET_SELECTION:
                    this.subPhaseManager.SwitchSubPhase(SubPhaseState.READY_TO_EXECUTE_MOVE, unitIndexOnStation);
                    break;

                case SubPhaseState.READY_TO_EXECUTE_MOVE:

                    /*  
                     *  If we have no more Intentions to resolve, check to see if all intents have been filled out. If so, proceed to combat. 
                     *  If not, we are likely in the StartOfRound Phase and so we want to move onto the Unit Turn Phase to proceed with Player-Driven Input.    
                     */

                    if (!this.combatRoundIntentionManager.IsAllUnitIntentionsComplete)
                    {
                        this.combatRoundIntentionManager.ProcessNextIntentionInSequence();
                    }
                    else
                    {
                        if (!Intention.UnitIntentionManager.Instance.AreUnitIntentionsDone)
                        {
                            this.phaseManager.ChangeToNextStateInOrder();
                        }
                        else
                        {
                            MonoBehaviour.print("<color=black>No one left to resolve. Switching to Resolve Attack</color>");
                            this.subPhaseManager.SwitchSubPhase(SubPhaseState.RESOLVE_ATTACK, unitIndexOnStation);
                        }
                    }

                    break;

                case SubPhaseState.RESOLVE_ATTACK:
                    this.subPhaseManager.SwitchSubPhase(SubPhaseState.ATTACK_COMPLETE, unitIndexOnStation);
                    break;

                case SubPhaseState.ATTACK_COMPLETE:
                    /*  
                     *  When called, an attack is done. 
                     *  However, determine if All intentions are done, or if it was a one-off attack from a unit. 
                     *  
                     *  In the event of the former, move to end of round phase. 
                     *  
                     *  In the case of the latter, check to see if the move ends their turn. If so, add them to the completed turn order or whatever.
                     *  
                     */

                    /*  Check to see if the Turn order list is empty. If not, we don't want to move to the end of round Phase.  */


                    /*  
                    *  IMPORTANT: THIS CONNECTION NEEDS TO BE REVISED. WE SHOULD NOT BE GOING INTO A SUBPHASE FROM WITHIN A PHASE.
                    *  WE SHOULD BE TOLD TO BY THIS CLASS!!!
                    */
                    if (TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count <= 0)
                    {
                        this.phaseManager.ChangeState(CombatTurnOrchestrationPhase.EndOfRound);
                    }


                    break;

                default:
                    break;
            }

        }


    }
}