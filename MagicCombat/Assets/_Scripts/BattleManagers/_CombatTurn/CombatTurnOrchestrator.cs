namespace TurnBased.Phases {

    public class CombatTurnOrchestrator
    {
        private Intention.CombatRoundUnitIntentionManager combatRoundIntentionManager   = new();
        private PhaseManager phaseManager                                               = new();
        private SubPhaseManager subPhaseManager                                         = new();
        private TurnOrder.TurnOrderManager TurnOrderManager                             = new();

        private Health.UnitHealthManager UnitHealthManager                              = new();
        private Status.UnitStatusManager UnitStatusManager                              = new();
        private Intention.UnitIntentionManager UnitIntentionManager                     = new();

        private Combat.TurnOrderCombatHandler TurnOrderCombatHandler                    = new();

        private Intention.IntentionResolverManager IntentionResolverManager             = new();
        private Combat.IntentionCombatResolver IntentionCombatResolver                  = new(); 

        private MoveSelection.MoveSelectorManager MoveSelectorManager                   = new();
        private TargetSelection.UnitTargetSelectorManager UnitTargetSelectorManager     = new();

        private AttackResolution.AttackResolutionManager AttackResolutionManager        = new();

        private PhaseTaskCompletionManager phaseCompletionManager;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the Battle. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfBattle;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the Round. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfRound;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the Start of the Player's Pre-Turn. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfPrePlayerTurn;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other to the Start of the Player Turn. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartPlayerTurn;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the resolution of Turn Order. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnTurnOrderResolving;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when Resolving Turn Order. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnResolvingTurnOrder;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the end of the Round. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnEndOfRound;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the end of the Battle. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnEndOfBattle;

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is awaiting Move Selection. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnAwaitingUnitMoveSelection;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is awaiting Target Selection. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnAwaitingUnitTargetSelection;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is ready to execute their Move. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnUnitReadyToExecuteMove;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is about to Resolve their move. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnUnitResolveMove;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit's Attack is complete. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnUnitAttackComplete;


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

        public void Awake()
        {
            UnityEngine.Debug.LogError("AWAKE CALLED INSIDE ORCHESTRATOR");
            GameState.GameStateManager.OnGameStateUpdated += GameStateManager_OnGameStateUpdated;

            currentOrchestrationPhase = CombatTurnOrchestrationPhase.StartOfBattle;

            this.combatRoundIntentionManager = new();
            this.phaseManager = new();
            this.subPhaseManager = new();
            this.TurnOrderManager = new();  
            this.UnitHealthManager = new();
            this.UnitStatusManager = new();
            this.UnitIntentionManager = new();
            this.TurnOrderCombatHandler = new();
            this.IntentionResolverManager = new();
            this.IntentionCombatResolver = new();
            this.MoveSelectorManager = new();
            this.UnitTargetSelectorManager = new();
            this.AttackResolutionManager = new();


            this.combatRoundIntentionManager.Awake(this);
            this.phaseManager.Awake(this.combatRoundIntentionManager, OnPhaseComplete);
            this.subPhaseManager.Awake(OnSubPhaseComplete);
            this.TurnOrderManager.Awake();

            this.UnitHealthManager.Awake();
            this.UnitStatusManager.Awake();
            this.UnitIntentionManager.Awake();

            this.TurnOrderCombatHandler.Awake();

            this.IntentionResolverManager.Awake();
            this.IntentionCombatResolver.Awake();

            this.MoveSelectorManager.Awake();
            this.UnitTargetSelectorManager.Awake();

            this.AttackResolutionManager.Awake();

        }

        public void OnDestroy()
        {
            TurnBased.GameState.GameStateManager.OnGameStateUpdated -= GameStateManager_OnGameStateUpdated;

            this.phaseManager.OnDestroy();
            this.subPhaseManager.OnDestroy();
            this.TurnOrderManager.OnDestroy();

            this.UnitHealthManager.OnDestroy();
            this.UnitStatusManager.OnDestroy();
            this.UnitIntentionManager.OnDestroy();

            this.TurnOrderCombatHandler.OnDestroy();

            this.IntentionResolverManager.OnDestroy();
            this.IntentionCombatResolver.OnDestroy();

            this.MoveSelectorManager.OnDestroy();
            this.UnitTargetSelectorManager.OnDestroy();

            this.AttackResolutionManager.OnDestroy();
            this.combatRoundIntentionManager.OnDestroy();



            this.phaseManager = null;
            this.subPhaseManager = null;
            this.TurnOrderManager = null;

            this.UnitHealthManager = null;
            this.UnitIntentionManager = null;

            this.TurnOrderCombatHandler = null;

            this.IntentionResolverManager = null;
            this.IntentionCombatResolver = null;    

            this.MoveSelectorManager = null;
            this.UnitTargetSelectorManager = null;

            this.AttackResolutionManager = null;
            this.combatRoundIntentionManager = null;
            
            
            /*  
             *  Remove all listeners to events. Doesn't prevent memory leaks, all listeners still need to unsubscribe.  
             *  But it resets it for next time.
             */
            OnStartOfBattle                 = null;
            OnStartOfRound                  = null;
            OnStartOfPrePlayerTurn          = null;
            OnStartPlayerTurn               = null;
            OnTurnOrderResolving            = null;
            OnResolvingTurnOrder            = null;
            OnEndOfRound                    = null;
            OnEndOfBattle                   = null;

            OnAwaitingUnitMoveSelection     = null;
            OnAwaitingUnitTargetSelection   = null;
            OnUnitReadyToExecuteMove        = null;
            OnUnitResolveMove               = null;
            OnUnitAttackComplete            = null;

        }

        public void Update()
        {
            this.phaseManager.Update();
            this.subPhaseManager.Update();

            string unitIndexDebuggingString = this.subPhaseManager.GetSelectedIndex() != null ? 
                $"<color=green> For UnitIndex: {this.subPhaseManager.GetSelectedIndex().Value.Index} </color>" : string.Empty;

            UnityEngine.MonoBehaviour.print($"<color=black>Current main phase state is: {currentOrchestrationPhase.ToString()}.</color> " +
                $"<color=white> Current sub phase: {this.subPhaseManager.CurrentSubPhaseState.ToString()} </color>" +
                unitIndexDebuggingString);

        }

        public void Start()
        {
            this.phaseManager.ChangeState(CombatTurnOrchestrationPhase.StartOfBattle);
        }

        private void GameStateManager_OnGameStateUpdated(MetaGameState currentGameState)
        {
            if (currentGameState == MetaGameState.Running) { return; }
            else
            {
                this.subPhaseManager.SwitchSubPhase(SubPhaseState.NONE, new());
                this.phaseManager.ChangeState(CombatTurnOrchestrationPhase.EndOfBattle);
            }
        }

        public void ChangeSubPhase(SubPhaseState subPhaseToGoInto, UnitIndex unitResolvingSubPhase)
        {
            this.subPhaseManager.SwitchSubPhase(subPhaseToGoInto, unitResolvingSubPhase);
        }


        private void OnPhaseComplete(CombatTurnOrchestrationPhase phaseThatCompleted)
        {
            switch (phaseThatCompleted)
            {
                case CombatTurnOrchestrationPhase.StartOfBattle:

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.StartOfRound;     
                    break;


                case CombatTurnOrchestrationPhase.StartOfRound:
                    UnityEngine.Debug.LogWarning($"Start of round phase is complete. Moving to pre-player turn.   ");

                    /*  Reset the subphase. */
                    this.subPhaseManager.SwitchSubPhase(SubPhaseState.NONE, new UnitIndex(0));
                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.PrePlayerTurn;
                    break;


                case CombatTurnOrchestrationPhase.PrePlayerTurn:

                    UnityEngine.Debug.LogWarning($"Pre player turn done. Moving to the player's main turn.   ");

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.PlayerTurn;
                    break;


                case CombatTurnOrchestrationPhase.PlayerTurn:

                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.TurnOrderRes;
                    break;

                case CombatTurnOrchestrationPhase.TurnOrderRes:

                    UnityEngine.Debug.LogWarning($"Turn order resolved. Moving to end of round.  Phase Manager  is:  {this.phaseManager}");
                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.EndOfRound;

                    break;

                case CombatTurnOrchestrationPhase.EndOfRound:

                    UnityEngine.Debug.LogWarning($"End of round over. Going to the start of round.   ");
                    CurrentOrchestrationPhase = CombatTurnOrchestrationPhase.StartOfRound;

                    break;


                default:
                    break;
            }

            this.phaseManager.ChangeState(CurrentOrchestrationPhase);
        }



        private void OnSubPhaseComplete(SubPhaseState phaseThatCompleted)
        {
            /*  Retrieve the selected station.  */
            StationIndex selectedStation = StationSelectorManager.Instance.GetSelectedStationIndex();

            /*  Try and get the Unit index on that station to pass to the subphase we are entering. */
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStation, out UnitIndex unitIndexOnStation)) { return; }

            /*  If User Input was awaited for purposes of Move or Target selection, complete it when the SubPhase is complete.  */
            this.combatRoundIntentionManager.CompleteAwaitingUserInput(unitIndexOnStation);


            switch (phaseThatCompleted)
            {
                case SubPhaseState.AWAITING_MOVE_SELECTION:
                    UnityEngine.Debug.LogWarning($"Move Selection Subphase is complete, processing the intention once more to aim to go to target resolution.   ");

                    /*  Get the intention of the Unit. If the Move Intent is done, we will be moved to Target Selection.    */
                    this.combatRoundIntentionManager.ProcessIntentionOfResolvingUnit();
                    break;

                case SubPhaseState.AWAITING_TARGET_SELECTION:
                    UnityEngine.Debug.LogWarning($"Target selection Subphase is complete, processing the intention once more to aim to go to ready to execute phase.   ");
                    /*  Get the intention of the Unit. If the Target Intent is done, we will be moved to Ready-To-Execute Move.    */
                    this.combatRoundIntentionManager.ProcessIntentionOfResolvingUnit();
                    break;

                case SubPhaseState.READY_TO_EXECUTE_MOVE:

                    UnityEngine.Debug.LogWarning($"Ready to execute was done.  ");

                    /*  First check to see if all the intents are done. If so, move to the next Main Phase in sequence. */
                    if (this.combatRoundIntentionManager.IsAllUnitIntentionsComplete)
                    {
                        UnityEngine.Debug.LogWarning($"ALL INTENTIONS DONE!!  ");

                        this.subPhaseManager.ResetCurrentPhase();
                        this.phaseManager.ChangeToNextStateInOrder();
                    }

                    /*  If the intentions are not yet done, Process the next unit in resolution order.  */
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Processing the next intention in sequence!!  ");

                        this.combatRoundIntentionManager.ProcessNextIntentionInSequence();
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