using System.Linq;
using TurnBased.Intention;
namespace TurnBased.Combat
{
    public static class IntentionCombatResolver
    {
        public readonly struct UnitDataForCombatResolution
        {
            public UnitData SourceUnitData { get; }
            public System.Collections.Generic.List<UnitData> AllyUnitData { get; }
            public System.Collections.Generic.List<UnitData> TargetUnitData { get; }
            public SceneData_UnitTurn SceneUnitData { get; }

            public UnitDataForCombatResolution(SceneData_UnitTurn sceneData, UnitData sourceData, System.Collections.Generic.List<UnitData> allyData, System.Collections.Generic.List<UnitData> targetData)
            {
                this.SceneUnitData = sceneData;
                this.SourceUnitData = sourceData;
                this.AllyUnitData = allyData;
                this.TargetUnitData = targetData;
            }
        }


        public static async System.Threading.Tasks.Task ProcessAttack(UnitIndex unitIndex)
        {
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO RETRIEVE INTENTION OF UNIT_INDEX!"); return; }

            UnityEngine.Debug.LogWarning($"Trying to exectute Move named: {intention.MoveSelection.GetMoveName()}! ");

            /*  Obtain the Unit Data for resolving this attack. */
            if (!TryGetUnitDataForCombatResolution(unitIndex, out var UnitDataForCombatResolution)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO OBTAIN UNIT DATA FOR UNIT_INDEX!"); return; }
            SceneData_UnitTurn sceneData = UnitDataForCombatResolution.SceneUnitData;

            /*  Execute the selected move by the User.  */
            if (!ExecuteMove(intention, UnitDataForCombatResolution, out AttackResolutionInfo exectutedMoveResolutionInfo)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO EXECUTE SELECTED MOVE!"); return; }


            /*  Determine if the Attack moves the user or not.  */
            await BattlePresentationManager.Instance.MoveUnitToTarget(sceneData.SourceStationIndex, intention.TargetIndexList.FirstOrDefault());

            /*  Process each step individually   */
            for (int i = 0; i < exectutedMoveResolutionInfo.Steps.Count; i++)
            {
                AttackResolution.CombatAttackHandler.ProcessAttackStep(exectutedMoveResolutionInfo, intention);
                UnityEngine.Debug.LogWarning($"Processed attack step for UnitIndex: {unitIndex.Index}");
            }

            /*  Move the user back.  */
            await BattlePresentationManager.Instance.MoveUnitToStation(sceneData.SourceStationIndex);

            /*  Clear the intention of the attack once done.    */
            Intention.UnitIntentionManager.Instance.ClearIntention(unitIndex);

            AttackResolution.UnitDeathResolver.DetermineDeadUnits();
        }
        
        public static bool TryGetUnitDataForCombatResolution(UnitIndex sourceUnitIndex, out UnitDataForCombatResolution outUnitDataForCombatResolution)
        {
            outUnitDataForCombatResolution = default;

            /*  Get the unit data for the User, their Allies, their enemies.    */
            SceneData_UnitTurn sceneUnitData = StationManagerUtilities.CreateCombatSceneDataForUnitIndex(sourceUnitIndex);

            if (!StationManager.Instance.TryGetUnitDataOnStation(sceneUnitData.SourceStationIndex, out UnitData unitDataSource)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO RETRIEVE SCENE UNIT DATA OF USER!"); return false; }
            System.Collections.Generic.List<UnitData> ally_UnitData = StationManagerUtilities.GetUnitDataOfStationIndexes(sceneUnitData.AllyStationIndexes);
            System.Collections.Generic.List<UnitData> enemyUnitData = StationManagerUtilities.GetUnitDataOfStationIndexes(sceneUnitData.EnemyStationIndexes);


            outUnitDataForCombatResolution = new UnitDataForCombatResolution(sceneUnitData, unitDataSource, ally_UnitData, enemyUnitData);
            return true;
        }

        public static bool ExecuteMove(UnitIntention userIntention, UnitDataForCombatResolution unitDataForCombatResolution, out AttackResolutionInfo exectutedMoveResolutionInfo)
        {
            /*  Process the move based on the information for that UnitIndex.   */
            exectutedMoveResolutionInfo = userIntention.MoveSelection.ExecuteMove(unitDataForCombatResolution.SourceUnitData, unitDataForCombatResolution.AllyUnitData, unitDataForCombatResolution.TargetUnitData);
            if (exectutedMoveResolutionInfo == null) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: RESOLUTION INFO OF MOVE IS NULL!"); return false; }

            UnityEngine.Debug.LogWarning($"Executing Move: {userIntention.MoveSelection.GetMoveName()}! ");
            return true;
        }

    }
}