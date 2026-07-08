namespace TurnBased.Combat
{
    public class IntentionCombatResolver
    {
        public static event System.Action OnResolvingStatesComplete;
        public static event System.Action<AttackResolution.ResolvingStatePhaseCompletionManager, Intention.ResolvingState> OnUnitResolvingState_BeforeAttack, OnUnitResolvingState_AfterAttack;
        public static event System.Action<AttackResolution.ResolvingStatePhaseCompletionManager, Intention.ResolvingState> OnEnvironmentResolvingState_BeforeAttack, OnEnvironmentResolvingState_AfterAttack;
        public static event System.Action<AttackResolution.ResolvingStatePhaseCompletionManager, Intention.ResolvingState> OnStatusResolvingState_BeforeAttack, OnStatusResolvingState_AfterAttack;


        private static IntentionCombatResolver instance;
        public static IntentionCombatResolver Instance
        {
            get
            {
                return instance;
            }
        }

        private AttackResolution.CombatAttackHandler _CombatAttackHandler;

        private EventHookSystem eventHookSystem;

        private System.Collections.Generic.LinkedList<AttackResolution.CombatResolvingRequest> resolvingRequests;
        private AttackResolution.CombatResolvingRequest currentResolvingRequest;
        private AttackResolution.ResolvingStatePhaseCompletionManager completionManager;
        private bool isResolving;

        public void Awake(EventHookSystem evHookSystem)
        {
            if (instance == null)
            {
                instance = this;
            }

            this.eventHookSystem = evHookSystem;

            this.resolvingRequests = new();
            this.isResolving = false;

            this._CombatAttackHandler = new();
            this._CombatAttackHandler.Awake(evHookSystem);
        }

        public void Update()
        {
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.eventHookSystem = null;
            this.resolvingRequests.Clear();
            this.resolvingRequests = null;

            OnResolvingStatesComplete = null;

            this._CombatAttackHandler = null;
        }

        public readonly struct UnitDataForCombatResolution
        {
            public UnitData SourceUnitData { get; }
            public System.Collections.Generic.List<UnitData> AllyUnitData { get; }
            public System.Collections.Generic.List<UnitData> TargetUnitData { get; }
            public UnitTurnStationIndexesSceneData SceneUnitData { get; }

            public UnitDataForCombatResolution(UnitTurnStationIndexesSceneData sceneData, UnitData sourceData, System.Collections.Generic.List<UnitData> allyData, System.Collections.Generic.List<UnitData> targetData)
            {
                this.SceneUnitData = sceneData;
                this.SourceUnitData = sourceData;
                this.AllyUnitData = allyData;
                this.TargetUnitData = targetData;
            }
        }

        public void AddResolvingStateToBack(AttackResolution.CombatResolvingRequest resolvingRequest)
        {
            this.resolvingRequests.AddLast(resolvingRequest);
            UnityEngine.Debug.LogWarning("Adding resolving state to back");

            ProcessNextRequest();
        }

        public void AddResolvingStateToFront(AttackResolution.CombatResolvingRequest resolvingRequest)
        {
            UnityEngine.Debug.LogWarning("Adding resolving state to front");
            if (this.resolvingRequests.Count <= 0)
            {
                this.resolvingRequests.AddFirst(resolvingRequest);
            }
            else
            {
                /*  Get the first node of this linked list to insert this resolving state into behind it.   */
                System.Collections.Generic.LinkedListNode<AttackResolution.CombatResolvingRequest> firstNode = this.resolvingRequests.First;
                this.resolvingRequests.AddAfter(firstNode, resolvingRequest);
            }


            ProcessNextRequest();
        }

        private void ProcessNextRequest()
        {
            if (this.isResolving) { return; }

            /*  Get the first resolving state to process.   */
            System.Collections.Generic.LinkedListNode<AttackResolution.CombatResolvingRequest> firstNode = this.resolvingRequests.First;


            if (firstNode == null)
            {
                UnityEngine.Debug.LogWarning($"First node is null");

                RemoveFrontResolvingState();
            }
            else
            {
                ProcessRequest(firstNode.Value);
            }
        }

        private void RemoveFrontResolvingState()
        {
            this.isResolving = false;
            this.resolvingRequests.RemoveFirst();

            if (this.resolvingRequests.Count > 0)
            {
                UnityEngine.Debug.LogError("CONTINUING RESOLVING STATES!");
                ProcessNextRequest();
            }
            else
            {
                UnityEngine.Debug.LogError("RESOLVING STATES DONE!");
                OnResolvingStatesComplete?.Invoke();
            }
        }


        private void ProcessRequest(AttackResolution.CombatResolvingRequest resolvingRequest)
        {
            this.isResolving = true;
            this.currentResolvingRequest = resolvingRequest;
            switch (resolvingRequest.ResolvingState.ResolvingSource.Type)
            {
                case DamageOriginType.UnitMove:

                    UnityEngine.Debug.LogWarning($"Processing Unit resolving state of unit index: {resolvingRequest.ResolvingState.ResolvingSource.SourceUnitIndex}");

                    ProcessUnitResolvingState_BeforeAttack();
                    break;

                case DamageOriginType.Status:
                    ProcessStatusResolvingState_BeforeAttack();

                   // await ProcessStatusResolvingState(resolvingRequest.ResolvingState);
                    break;

                case DamageOriginType.Environment:
                    ProcessEnvironmentResolvingState_BeforeAttack();
                    break;

                default:
                    break;
            }

           // OnCombatResolvingRequestComplete();
        }

        private void OnCombatResolvingRequestComplete()
        {
            this.currentResolvingRequest.CompleteRequest();
            this.currentResolvingRequest = null;
            RemoveFrontResolvingState();
        }


        //private async System.Threading.Tasks.Task ProcessUnitResolvingState(Intention.ResolvingState resolvingState)
        //{
        //    UnitIndex sourceUnitIndex = resolvingState.ResolvingSource.SourceUnitIndex;

        //    UnityEngine.Debug.LogWarning($"source unit index is: {sourceUnitIndex.Index}");


        //    UnityEngine.Debug.LogWarning($"Executing attack action in order inside process attack. Is there a valid target?    ");

        //    /*  If there is a targeted unit, proceed */
        //    UnityEngine.Debug.LogWarning($"There is a valid target moving to target");

        //    /*  Determine if the Attack moves the user or not.  */
        //    await Presentation.BattlePresentationManager.Instance.VisualiseUnitTeleportUserAndTargets(resolvingState);

        //    UnityEngine.Debug.LogWarning($"Processing attack step");


        //    /*  Process each step individually   */

        //    await this._CombatAttackHandler.ProcessAttackStep(resolvingState);


        //    UnityEngine.Debug.LogWarning($"Moving back to station");

        //    /*  Move the user back.  */
        //    await Presentation.BattlePresentationManager.Instance.ReturnSourceAndTargetsBackToStations(resolvingState);

        //    UnityEngine.Debug.LogWarning($"Clearing intention");



        //    /*  Clear the intention of the attack once done.    */
        //    Intention.UnitIntentionManager.Instance.ClearIntention(sourceUnitIndex, resolvingState.ResolvingSource.SourceUnitMove);

        //    UnityEngine.Debug.LogWarning($"Determining dead units");

        //    GameState.GameStateManager.Instance.DetermineDeadUnits();

        //}

        #region Unit Process Sequence

        /*  
        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            UNIT PROCESS SEQUENCE
        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=         
         */

        private void ProcessUnitResolvingState_BeforeAttack()
        {
            // After we teleport or don't teleport the player and targets depending on the move, we want to wait for any things that want to happen after this.
            this.completionManager = new(ProcessUnitResolvingState_Attack);

            this.completionManager.AddAction();
            OnUnitResolvingState_BeforeAttack?.Invoke(this.completionManager, this.currentResolvingRequest.ResolvingState);
            this.completionManager.OnActionComplete();
        }
        private void ProcessUnitResolvingState_Attack()
        {
            // We want to play the animation for this move and the units. When a specific part in the animation occours, then we want to process the attack step. 
            // Unlike previously, we want to check when all attack steps are resolved. I.e. each attack step inside the handler should track if they are resolved or not. When all attack steps are fully complete then we can move on.
            // This gives the opportunity to handle animations from the targets upon taking damage, particle effects with delayed triggers to deal damage or heal, etc. Once all of that is done, then we should move on to after the attack is done. 
            

            this._CombatAttackHandler.StartProcessingResolvingState(this.currentResolvingRequest.ResolvingState, ProcessUnitResolvingState_AfterAttack);
        }
        private void ProcessUnitResolvingState_AfterAttack()
        {
            this.completionManager = new(OnCompleteUnitResolvingState);

            this.completionManager.AddAction();
            OnUnitResolvingState_AfterAttack?.Invoke(this.completionManager, this.currentResolvingRequest.ResolvingState);
            this.completionManager.OnActionComplete();
        }

        private void OnCompleteUnitResolvingState()
        {
            UnitIndex sourceUnitIndex = this.currentResolvingRequest.ResolvingState.ResolvingSource.SourceUnitIndex;

            /*  Clear the intention of the attack once done.    */
            Intention.UnitIntentionManager.Instance.ClearIntention(sourceUnitIndex, this.currentResolvingRequest.ResolvingState.ResolvingSource.SourceUnitMove);
            GameState.GameStateManager.Instance.DetermineDeadUnits();

            OnCombatResolvingRequestComplete();
        }

        #endregion

        #region Status Process Sequence

        /*  
        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            STATUS PROCESS SEQUENCE
        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=         
         */

        private void ProcessStatusResolvingState_BeforeAttack()
        {
            // Process some kind of event if existant before status.
            this.completionManager = new(ProcessStatusResolvingState_Attack);

            this.completionManager.AddAction();
            OnStatusResolvingState_BeforeAttack?.Invoke(this.completionManager, this.currentResolvingRequest.ResolvingState);
            this.completionManager.OnActionComplete();
        }

        private void ProcessStatusResolvingState_Attack()
        {
            this._CombatAttackHandler.StartProcessingResolvingState(this.currentResolvingRequest.ResolvingState, ProcessStatusResolvingState_AfterAttack);
        }


        private void ProcessStatusResolvingState_AfterAttack()
        {
            this.completionManager = new(OnCompleteStatusResolvingState);

            this.completionManager.AddAction();
            OnUnitResolvingState_AfterAttack?.Invoke(this.completionManager, this.currentResolvingRequest.ResolvingState);
            this.completionManager.OnActionComplete();
        }

        private void OnCompleteStatusResolvingState()
        {
            GameState.GameStateManager.Instance.DetermineDeadUnits();

            OnCombatResolvingRequestComplete();
        }

        #endregion

        #region Environment Process Sequence

        /*  
        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            ENVIRONMENT PROCESS SEQUENCE
        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=         
         */

        private void ProcessEnvironmentResolvingState_BeforeAttack()
        {
            // Process some kind of event if existant before status.
            this.completionManager = new(ProcessEnvironmentResolvingState_Attack);

            this.completionManager.AddAction();
            OnEnvironmentResolvingState_BeforeAttack?.Invoke(this.completionManager, this.currentResolvingRequest.ResolvingState);
            this.completionManager.OnActionComplete();
        }

        private void ProcessEnvironmentResolvingState_Attack()
        {
            this._CombatAttackHandler.StartProcessingResolvingState(this.currentResolvingRequest.ResolvingState, ProcessEnvironmentResolvingState_AfterAttack);
        }


        private void ProcessEnvironmentResolvingState_AfterAttack()
        {
            this.completionManager = new(OnCompleteProcessEnvironmentResolvingState);

            this.completionManager.AddAction();
            OnEnvironmentResolvingState_AfterAttack?.Invoke(this.completionManager, this.currentResolvingRequest.ResolvingState);
            this.completionManager.OnActionComplete();
        }

        private void OnCompleteProcessEnvironmentResolvingState()
        {
            GameState.GameStateManager.Instance.DetermineDeadUnits();

            OnCombatResolvingRequestComplete();
        }

        #endregion
        
        public static bool TryGetUnitDataForCombatResolution(UnitIndex sourceUnitIndex, out UnitDataForCombatResolution outUnitDataForCombatResolution)
        {
            outUnitDataForCombatResolution = default;

            /*  Get the unit data for the User, their Allies, their enemies.    */
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(sourceUnitIndex, out UnitTurnStationIndexesSceneData sceneUnitData)) { return false; }

            if (!StationManager.Instance.TryGetUnitDataOnStation(sceneUnitData.SourceStationIndex, out UnitData unitDataSource)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO RETRIEVE SCENE UNIT DATA OF USER!"); return false; }
            System.Collections.Generic.List<UnitData> ally_UnitData = StationManagerUtilities.GetUnitDataOfStationIndexes(sceneUnitData.AllyStationIndexes);
            System.Collections.Generic.List<UnitData> enemyUnitData = StationManagerUtilities.GetUnitDataOfStationIndexes(sceneUnitData.EnemyStationIndexes);


            outUnitDataForCombatResolution = new UnitDataForCombatResolution(sceneUnitData, unitDataSource, ally_UnitData, enemyUnitData);
            return true;
        }
    }


    public static class IntentionCombatResolverUtility
    {
        public static AttackResolution.CombatResolvingRequest AddToCombatResolverBack(Intention.ResolvingState resolvingState)
        {
            AttackResolution.CombatResolvingRequest request = new(resolvingState);
            IntentionCombatResolver.Instance.AddResolvingStateToBack(request);

            return request;
        }
        public static AttackResolution.CombatResolvingRequest AddToCombatResolverFront(Intention.ResolvingState resolvingState)
        {
            UnityEngine.Debug.LogError($"Adding resolving state to front! {resolvingState.ResolvingSource.Type}");
            AttackResolution.CombatResolvingRequest request = new(resolvingState);
            IntentionCombatResolver.Instance.AddResolvingStateToFront(request);
            UnityEngine.Debug.LogError($"Added Resolving state to front!");


            return request;
        }
    }
}