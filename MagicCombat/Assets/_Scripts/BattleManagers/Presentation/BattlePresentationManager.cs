using TurnBased.Combat;
using UnityEngine;

namespace TurnBased.Presentation
{
    public class BattlePresentationManager : MonoBehaviour
    {

        private static BattlePresentationManager instance;
        public static BattlePresentationManager Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (instance == null)
                {
                    instance = value;
                }
            }
        }

        private AttackResolution.ResolvingStatePhaseCompletionManager _CompletionManager;
        private ParticleSystemManager particleSystemManager;
        private AnimationPresentationManager animationPresentationManager;

        [Header("Inspector Variables")]
        [SerializeField] private ParticlesCollection_SO particlesCollectionData;
        


        [SerializeField] GameObject tempVisual;
        private Vector3 ALLY_COMBAT_LOCATION = new(0,0,-2f), ENEMY_COMBAT_LOCATION = new(0, 0, -4f);

        private readonly System.Collections.Generic.List<Vector2> LOCATION_MODIFIERS = new System.Collections.Generic.List<Vector2>
        {   new(0,  0),     new(0, -1),     new(-1,-1),
            new(-1, 1),     new(-1, 0),     new(1,  0),
            new(0,  1),     new(-1, 1),     new(1,  1)
        };

        private const float LOCATION_MODIFIER_DISTANCE = 1.25f;

        private void Awake()
        {
            Instance = this;

            StationManager.OnDeployUnit += StationManager_OnDeployUnit;
            StationSelectorManager.OnSelectionChange += StationSelectorManager_OnSelectionChange;
            IntentionCombatResolver.OnUnitResolvingState_BeforeAttack += IntentionCombatResolver_OnUnitResolvingState_BeforeAttack;
            IntentionCombatResolver.OnUnitResolvingState_AfterAttack += IntentionCombatResolver_OnUnitResolvingState_AfterAttack; 


            this.particleSystemManager = new();
            this.particleSystemManager.Awake(this.particlesCollectionData);

            this.animationPresentationManager = new();
            this.animationPresentationManager.Awake();
        }

        private void Start()
        {
            StationSelectorManager_OnSelectionChange(StationSelectorManager.Instance.GetSelectedStationIndex(), null);
        }

        private void OnDestroy()
        {
            if (Instance != null && Instance == this)
            {
                Instance = null;
            }
            StationManager.OnDeployUnit -= StationManager_OnDeployUnit;
            StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
            IntentionCombatResolver.OnUnitResolvingState_BeforeAttack -= IntentionCombatResolver_OnUnitResolvingState_BeforeAttack;
            IntentionCombatResolver.OnUnitResolvingState_AfterAttack -= IntentionCombatResolver_OnUnitResolvingState_AfterAttack;

            this.particleSystemManager.OnDestroy();
            this.particleSystemManager = null;

            this.animationPresentationManager.OnDestroy();
            this.animationPresentationManager = null;
        }

        private void StationManager_OnDeployUnit(StationIndex stationIndex, UnitIndex deployUnitIndex, UnitIndex? recallUnitIndex)
        {
            /*  Get the station and the BaseBattleUnit */
            if (!StationManager.Instance.TryGetStationOfStationIndex(stationIndex, out Station stationOfStationIndex)) { return; }

            if (!StationManager.Instance.TryGetBattleUnitOfIndex(deployUnitIndex, out BaseBattleUnit battleUnit)) { return; }

            battleUnit.transform.position = stationOfStationIndex.Position;
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex? deselectedStationIndex)
        {
            UnityEngine.Debug.Log($" BattlePresentationManager  OnSelectionChange!");
            StationManagerUtilities.GetBattleUnitOnStation(selectedStationIndex, out BaseBattleUnit battleUnitOnStation);

            if (tempVisual != null)
            {
                tempVisual.transform.position = battleUnitOnStation.transform.position;
            }

            UnityEngine.Debug.Log($"Moved the visual!");
        }

        private void GetDeclaredTargetsFromResolvingState(Intention.ResolvingState resolvingState, out System.Collections.Generic.HashSet<StationIndex> targetStations)
        {
            targetStations = new();
            foreach (Intention.TargetGroupResolvingState targetGroup in resolvingState.TargetGroupResolvingStates)
            {
                foreach (StationIndex targetStationIndex in targetGroup.DeclaredTargets)
                {
                    targetStations.Add(targetStationIndex);
                }
            }
        }

        private void GetAllyAndEnemyLocationOnSourceTeam(UnitTeam team, out Vector3 sourceTransform, out Vector3 targetTransform)
        {
            if (team == UnitTeam.ENEMY)
            {
                sourceTransform = this.ENEMY_COMBAT_LOCATION;
                targetTransform = this.ALLY_COMBAT_LOCATION;
            }
            else
            {
                sourceTransform = this.ALLY_COMBAT_LOCATION;
                targetTransform = this.ENEMY_COMBAT_LOCATION;
            }
        }

        private void IntentionCombatResolver_OnUnitResolvingState_BeforeAttack(AttackResolution.ResolvingStatePhaseCompletionManager completionManager, Intention.ResolvingState resolvingState)
        {
            this._CompletionManager = completionManager;
            this._CompletionManager.AddAction();
            _ = ProcessUnitPresentationBeforeAttack(resolvingState);
        }

        private async System.Threading.Tasks.Task ProcessUnitPresentationBeforeAttack(Intention.ResolvingState resolvingState)
        {
            await VisualiseUnitTeleportUserAndTargets(resolvingState);
            this._CompletionManager.OnActionComplete();
        }

        public async System.Threading.Tasks.Task VisualiseUnitTeleportUserAndTargets(Intention.ResolvingState resolvingState)
        {
            /*  Get the baseBattleUnit of the source    */
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(resolvingState.ResolvingSource.SourceUnitIndex, out BaseBattleUnit sourceUnit)) { return; }
            if (!StationManager.Instance.TryGetTeamOfUnitIndex(resolvingState.ResolvingSource.SourceUnitIndex, out UnitTeam sourceTeam)) { return; }

            /*  Loop through all Target Groups and conglomerate all targets for this attack to teleport them in to the centre.  */
            GetDeclaredTargetsFromResolvingState(resolvingState, out System.Collections.Generic.HashSet<StationIndex> targetStations);

            /*  Get the locations for the source and the target based on the team.  */
            GetAllyAndEnemyLocationOnSourceTeam(sourceTeam, out Vector3 sourceTransform, out Vector3 targetTransform);

            /*  Put the targets on and around the targetTransform.  */
            int modifierIndex = 0;
            foreach (StationIndex targetStationIndex in targetStations)
            {
                if (!StationManager.Instance.TryGetBaseBattleUnitOnStation(targetStationIndex, out BaseBattleUnit targetUnit)) { continue; }
                Quaternion targetRotation = targetUnit.transform.rotation;

                /*  Put the target on the location around the target transform based on the modifier index. */
                Vector2 positionModifier = LOCATION_MODIFIERS[modifierIndex] * LOCATION_MODIFIER_DISTANCE;
                Vector3 newTargetPosition = new(targetTransform.x + positionModifier.x, 0, targetTransform.z + positionModifier.y);

                UnityEngine.Debug.LogError($"Moving Unit Named: {targetUnit.name} to position: {newTargetPosition.x}, {newTargetPosition.y}, {newTargetPosition.z}");

                targetUnit.transform.SetPositionAndRotation(newTargetPosition, targetRotation);
                modifierIndex++;
            }

            /*  Put the Source on their transform   */
            Quaternion sourceRotation = sourceUnit.transform.rotation;

            /*  Put the target on the location around the target transform based on the modifier index. */
            Vector3 newSourcePosition = new(sourceTransform.x, sourceTransform.y, sourceTransform.z);

            UnityEngine.Debug.LogError($"Moving Unit Named: {sourceUnit.name} to position: {newSourcePosition.x}, {newSourcePosition.y}, {newSourcePosition.z}");


            sourceUnit.transform.SetPositionAndRotation(newSourcePosition, sourceRotation);

            await System.Threading.Tasks.Task.Delay(1000);
        }

        private void IntentionCombatResolver_OnUnitResolvingState_AfterAttack(AttackResolution.ResolvingStatePhaseCompletionManager completionManager, Intention.ResolvingState resolvingState)
        {
            this._CompletionManager = completionManager;
            this._CompletionManager.AddAction();
            _ = ProcessUnitPresentationAfterAttack(resolvingState);
        }

        private async System.Threading.Tasks.Task ProcessUnitPresentationAfterAttack(Intention.ResolvingState resolvingState)
        {
            await ReturnSourceAndTargetsBackToStations(resolvingState);
            this._CompletionManager.OnActionComplete();
        }

        public async System.Threading.Tasks.Task ReturnSourceAndTargetsBackToStations(Intention.ResolvingState resolvingState)
        {
            /*  Get the baseBattleUnit of the source and Target */
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(resolvingState.ResolvingSource.SourceUnitIndex, out BaseBattleUnit sourceUnit)) { return; }

            /*  Loop through all Target Groups and conglomerate all targets for this attack to teleport them in to the centre.  */
            GetDeclaredTargetsFromResolvingState(resolvingState, out System.Collections.Generic.HashSet<StationIndex> targetStations);

            /*  Put the targets on and around the targetTransform.  */
            foreach (StationIndex targetStationIndex in targetStations)
            {
                if (!StationManager.Instance.TryGetStationOfStationIndex(targetStationIndex, out Station targetStation)) { continue; }

                if (!StationManager.Instance.TryGetBaseBattleUnitOnStation(targetStationIndex, out BaseBattleUnit targetUnit)) { continue; }
                Quaternion targetRotation = targetUnit.transform.rotation;

                targetUnit.transform.SetPositionAndRotation(targetStation.Position, targetRotation);
            }

            if (!StationManager.Instance.TryGetStationOfUnitIndex(resolvingState.ResolvingSource.SourceUnitIndex, out Station sourceStation)) { return; }

            /*  Put the Source on their transform   */
            Quaternion sourceRotation = sourceUnit.transform.rotation;

            sourceUnit.transform.SetPositionAndRotation(sourceStation.Position, sourceRotation);

            await System.Threading.Tasks.Task.Delay(1000);
        }
    }
}