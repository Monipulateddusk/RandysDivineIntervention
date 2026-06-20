using TurnBased.GameState;

namespace TurnBased.Combat
{
    public class IntentionCombatResolver
    {
        private static IntentionCombatResolver instance;
        public static IntentionCombatResolver Instance
        {
            get
            {
                return instance;
            }
        }

        public static event System.Action OnResolvingStatesComplete;

        private System.Collections.Generic.LinkedList<AttackResolution.CombatResolvingRequest> resolvingRequests;
        private bool isResolving;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.resolvingRequests = new();
            this.isResolving = false;
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

            this.resolvingRequests.Clear();
            this.resolvingRequests = null;

            OnResolvingStatesComplete = null;
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
                _ = ProcessRequest(firstNode.Value);
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


        private async System.Threading.Tasks.Task ProcessRequest(AttackResolution.CombatResolvingRequest resolvingRequest)
        {
            this.isResolving = true;
            switch (resolvingRequest.ResolvingState.ResolvingSource.Type)
            {
                case DamageOriginType.UnitMove:

                    UnityEngine.Debug.LogWarning($"Processing Unit resolving state of unit index: {resolvingRequest.ResolvingState.ResolvingSource.SourceUnitIndex}");


                    await ProcessUnitResolvingState(resolvingRequest.ResolvingState);

                    break;

                case DamageOriginType.Status:
                    await ProcessStatusResolvingState(resolvingRequest.ResolvingState);
                    break;

                case DamageOriginType.Environment:
                    await ProcessEnvironmentResolvingState(resolvingRequest.ResolvingState);
                    break;

                default:
                    break;
            }

            resolvingRequest.CompleteRequest();
            RemoveFrontResolvingState();
        }

        private async System.Threading.Tasks.Task ProcessUnitResolvingState(Intention.ResolvingState resolvingState)
        {
            UnitIndex sourceUnitIndex = resolvingState.ResolvingSource.SourceUnitIndex;

            UnityEngine.Debug.LogWarning($"source unit index is: {sourceUnitIndex.Index}");

            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(sourceUnitIndex, out Intention.UnitIntention intention)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO RETRIEVE INTENTION OF UNIT_INDEX!"); return; }

            UnityEngine.Debug.LogWarning($"Trying to exectute Move named: {resolvingState.ResolvingSource.SourceUnitMove.GetMoveName()}! ");

            /*  Obtain the Unit Data for resolving this attack. */
            if (!TryGetUnitDataForCombatResolution(sourceUnitIndex, out var UnitDataForCombatResolution)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO OBTAIN UNIT DATA FOR UNIT_INDEX!"); return; }
            UnitTurnStationIndexesSceneData sceneData = UnitDataForCombatResolution.SceneUnitData;

            UnityEngine.Debug.LogWarning($"Executing attack action in order inside process attack. Is there a valid target?    ");

            /*  If there is a targeted unit, proceed */
            UnityEngine.Debug.LogWarning($"There is a valid target moving to target");

            /*  Determine if the Attack moves the user or not.  */
            //await BattlePresentationManager.Instance.MoveUnitToTarget(sceneData.SourceStationIndex, intention.DeclaredTargetGroups.FirstOrDefault().Value.FirstOrDefault());

            UnityEngine.Debug.LogWarning($"Processing attack step");


            /*  Process each step individually   */

            await AttackResolution.CombatAttackHandler.ProcessAttackStep(intention.ResolvingState);


            UnityEngine.Debug.LogWarning($"Moving back to station");

            /*  Move the user back.  */
            //await BattlePresentationManager.Instance.MoveUnitToStation(sceneData.SourceStationIndex);

            UnityEngine.Debug.LogWarning($"Clearing intention");



            /*  Clear the intention of the attack once done.    */
            Intention.UnitIntentionManager.Instance.ClearIntention(sourceUnitIndex);

            UnityEngine.Debug.LogWarning($"Determining dead units");

            GameStateManager.Instance.DetermineDeadUnits();

        }

        private async System.Threading.Tasks.Task ProcessStatusResolvingState(Intention.ResolvingState resolvingState)
        {
            UnityEngine.Debug.LogError($"Processing Status resolving state!");
            await AttackResolution.CombatAttackHandler.ProcessAttackStep(resolvingState);
            UnityEngine.Debug.LogError($"Processed Status Resolving State!");

            GameStateManager.Instance.DetermineDeadUnits();
        }

        private async System.Threading.Tasks.Task ProcessEnvironmentResolvingState(Intention.ResolvingState resolvingState)
        {
            await AttackResolution.CombatAttackHandler.ProcessAttackStep(resolvingState);

            GameStateManager.Instance.DetermineDeadUnits();
        }
        
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