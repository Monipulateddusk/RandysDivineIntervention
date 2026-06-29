namespace TurnBased.Status
{
    public class CombatStatusHandler
    {
        private Phases.PhaseTaskCompletionManager completionManager;
        private static CombatStatusHandler instance;
        public static CombatStatusHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            EventHookSystem.OnStartOfRoundPhase          += EventHookSystem_OnStartOfRoundPhase;
            EventHookSystem.OnStartOfPrePlayerTurnPhase  += EventHookSystem_OnStartOfPrePlayerTurnPhase;
            EventHookSystem.OnStartOfPlayerTurnPhase     += EventHookSystem_OnStartOfPlayerTurn;
            EventHookSystem.OnResolvingTurnOrderPhase    += EventHookSystem_OnResolvingTurnOrder;
            EventHookSystem.OnEndOfRoundPhase            += EventHookSystem_OnEndOfRound;

        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            EventHookSystem.OnStartOfRoundPhase             -= EventHookSystem_OnStartOfRoundPhase;
            EventHookSystem.OnStartOfPrePlayerTurnPhase     -= EventHookSystem_OnStartOfPrePlayerTurnPhase;
            EventHookSystem.OnStartOfPlayerTurnPhase        -= EventHookSystem_OnStartOfPlayerTurn;
            EventHookSystem.OnResolvingTurnOrderPhase       -= EventHookSystem_OnResolvingTurnOrder;
            EventHookSystem.OnEndOfRoundPhase               -= EventHookSystem_OnEndOfRound;
        }


        private void EventHookSystem_OnStartOfRoundPhase(Phases.PhaseTaskCompletionManager completionManager)
        {
            AssignCompletionManager(completionManager);
            ProcessStatusAtPhase(CombatTurnOrchestrationPhase.StartOfRound);
        }

        private void EventHookSystem_OnStartOfPrePlayerTurnPhase(Phases.PhaseTaskCompletionManager completionManager)
        {
            AssignCompletionManager(completionManager);
            ProcessStatusAtPhase(CombatTurnOrchestrationPhase.PrePlayerTurn);
        }
        private void EventHookSystem_OnStartOfPlayerTurn(Phases.PhaseTaskCompletionManager completionManager)
        {
            AssignCompletionManager(completionManager);
            ProcessStatusAtPhase(CombatTurnOrchestrationPhase.PlayerTurn);
        }
        private void EventHookSystem_OnResolvingTurnOrder(Phases.PhaseTaskCompletionManager completionManager)
        {
            AssignCompletionManager(completionManager);
            ProcessStatusAtPhase(CombatTurnOrchestrationPhase.TurnOrderRes);
        }
        private void EventHookSystem_OnEndOfRound(Phases.PhaseTaskCompletionManager completionManager)
        {
            AssignCompletionManager(completionManager);
            ProcessStatusAtPhase(CombatTurnOrchestrationPhase.EndOfRound);
        }

        private void AssignCompletionManager(Phases.PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();

            Combat.IntentionCombatResolver.OnResolvingStatesComplete += IntentionCombatResolver_OnResolvingStatesComplete;
        }

        private void IntentionCombatResolver_OnResolvingStatesComplete()
        {
            Combat.IntentionCombatResolver.OnResolvingStatesComplete -= IntentionCombatResolver_OnResolvingStatesComplete;
            this.completionManager.OnActionComplete();
            this.completionManager = null;
        }


        public void AddStatusEffect(AttackResolution.ApplyStatusRequest request, BaseStatus statusAdded)
        {
            //  -=-=-=-=-=-=-=-=-
            //  Resolve the OnStatusAdded method to retieve all of the resolution information
            //  -=-=-=-=-=-=-=-=-
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(request.TargetUnit, out TurnBased.Information.ResolutionSceneData resolutionSceneData)) { return; }
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(request.TargetUnit, out UnitTurnStationIndexesSceneData stationIndexesSceneData)) { return; }

            AttackResolutionInfo onStatusAddedInfo = statusAdded.OnStatusAdded(resolutionSceneData);

            //  -=-=-=-=-=-=-=-=-
            //  Process the Target Selection of the Adding Status Events
            //  -=-=-=-=-=-=-=-=-
            Intention.ResolvingSource resolvingSource = new(statusAdded, request.TargetUnit);
            Intention.ResolvingState statusResolvingState = new(resolvingSource);

            if (!Intention.UnitIntentionFactory.BuildAttackActionResolvingState(request.TargetUnit, onStatusAddedInfo, statusResolvingState)) { return; }
            if (!GeneralPurposeTargettingManager.Instance.TryGetTargetSelector(statusAdded.StatusName, out TargetSelection.ITargetSelector targetSelector)) {  return; }    

            Intention.IntentionResolverManager.Instance.ProcessResolvingStateTargetSelection(stationIndexesSceneData, statusResolvingState, targetSelector);

            //  -=-=-=-=-=-=-=-=-
            //  Add Resolving state as a request to the resolver
            //  -=-=-=-=-=-=-=-=-
            AttackResolution.CombatResolvingRequest resolvingRequest = Combat.IntentionCombatResolverUtility.AddToCombatResolverFront(statusResolvingState);
        }
        public void RemoveStatusEffect(AttackResolution.RemoveStatusRequest request, BaseStatus statusRemoved)
        {
            //  -=-=-=-=-=-=-=-=-
            //  Resolve the OnStatusAdded method to retieve all of the resolution information
            //  -=-=-=-=-=-=-=-=-
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(request.TargetUnit, out TurnBased.Information.ResolutionSceneData resolutionSceneData)) { return; }
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(request.TargetUnit, out UnitTurnStationIndexesSceneData stationIndexesSceneData)) { return; }

            AttackResolutionInfo onStatusAddedInfo = statusRemoved.OnStatusRemoved(resolutionSceneData);

            //  -=-=-=-=-=-=-=-=-
            //  Process the Target Selection of the Adding Status Events
            //  -=-=-=-=-=-=-=-=-
            Intention.ResolvingSource resolvingSource = new(statusRemoved, request.TargetUnit);
            Intention.ResolvingState statusResolvingState = new(resolvingSource);

            if (!Intention.UnitIntentionFactory.BuildAttackActionResolvingState(request.TargetUnit, onStatusAddedInfo, statusResolvingState)) { return; }
            if (!GeneralPurposeTargettingManager.Instance.TryGetTargetSelector(statusRemoved.StatusName, out TargetSelection.ITargetSelector targetSelector)) { return; }

            Intention.IntentionResolverManager.Instance.ProcessResolvingStateTargetSelection(stationIndexesSceneData, statusResolvingState, targetSelector);

            //  -=-=-=-=-=-=-=-=-
            //  Add Resolving state as a request to the resolver
            //  -=-=-=-=-=-=-=-=-
            AttackResolution.CombatResolvingRequest resolvingRequest = Combat.IntentionCombatResolverUtility.AddToCombatResolverFront(statusResolvingState);
        }

        private void ProcessStatusAtPhase(CombatTurnOrchestrationPhase phaseTiming)
        {
            System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<(UnitIndex, BaseStatus)>> totalStatusResolutionDict = new();
            System.Collections.Generic.List<UnitIndex> units = StationManager.Instance.GetAllActiveUnits();

            /*  For each active Unit, Process their Status at this phase timing. We want to bundle all Status that is the same (burn, poison) in the same entry in the dictionary so it is all resolved correclty.  */
            foreach (UnitIndex unitIndex in units)
            {
                if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }
                System.Collections.Generic.List<BaseStatus> unitStatusDict = unitStatus.GetStatusesAtResolutionTiming(unitIndex, phaseTiming);

                /*  After we have all of the status for this Unit processed, push it into our dictionary so all the same typed status all resolve at the same time. */
                foreach (BaseStatus status in unitStatusDict)
                {
                    if (totalStatusResolutionDict.ContainsKey(status.GetType()))
                    {
                        totalStatusResolutionDict[status.GetType()].Add((unitIndex, status));
                    }
                    else
                    {
                        totalStatusResolutionDict.Add(status.GetType(), new() { (unitIndex, status) });
                    }
                }
            }

            /*  If there are no status to process, we don't want to be idling waiting for an event to never happen. */
            if (totalStatusResolutionDict.Count <= 0)
            {
                IntentionCombatResolver_OnResolvingStatesComplete();
                return;
            }            

            ProcessStatusToCombatResolver(totalStatusResolutionDict);
        }

        private void ProcessStatusToCombatResolver(System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<(UnitIndex, BaseStatus)>> unitStatusResolutionDictionary)
        {
            /*  Loop through each of the different kinds of status we recieved. */
            foreach (System.Collections.Generic.List<(UnitIndex, BaseStatus)> statusTypeUnitStatusList in unitStatusResolutionDictionary.Values)
            {
                /*  Loop through each Unit aflicted with this status and process the status.    */
                foreach ((UnitIndex, BaseStatus) statusOfUnit in statusTypeUnitStatusList)
                {
                    UnitIndex unitIndex = statusOfUnit.Item1;
                    BaseStatus status = statusOfUnit.Item2;

                    //  -=-=-=-=-=-=-=-=-
                    //  Resolve the ProcessStatus method to retieve all of the resolution information
                    //  -=-=-=-=-=-=-=-=-
                    if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(unitIndex, out TurnBased.Information.ResolutionSceneData resolutionSceneData)) { return; }
                    if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(unitIndex, out UnitTurnStationIndexesSceneData stationIndexesSceneData)) { return; }

                    AttackResolutionInfo processStatusInfo = status.ProcessStatus(resolutionSceneData);

                    //  -=-=-=-=-=-=-=-=-
                    //  Process the Target Selection of the Process Status Method
                    //  -=-=-=-=-=-=-=-=-
                    Intention.ResolvingSource resolvingSource = new(status, unitIndex);
                    Intention.ResolvingState statusResolvingState = new(resolvingSource);

                    if (!Intention.UnitIntentionFactory.BuildAttackActionResolvingState(unitIndex, processStatusInfo, statusResolvingState)) { return; }
                    if (!GeneralPurposeTargettingManager.Instance.TryGetTargetSelector(status.StatusName, out TargetSelection.ITargetSelector targetSelector)) { return; }

                    Intention.IntentionResolverManager.Instance.ProcessResolvingStateTargetSelection(stationIndexesSceneData, statusResolvingState, targetSelector);

                    //  -=-=-=-=-=-=-=-=-
                    //  Add Resolving state as a request to the resolver
                    //  -=-=-=-=-=-=-=-=-
                    AttackResolution.CombatResolvingRequest resolvingRequest = Combat.IntentionCombatResolverUtility.AddToCombatResolverFront(statusResolvingState);
                }
            }

        }
    }
}