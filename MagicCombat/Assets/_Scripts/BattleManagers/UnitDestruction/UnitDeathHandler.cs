using UnityEngine;

namespace TurnBased.LoaderUnloader
{
    public class UnitDeathHandler
    {
        private const float PHASE_OUT_DURATION = 0.5f;
        private const float PHASE_OUT_SHAKE_SPEED = 50.0f;
        private const float PHASE_OUT_DISTANCE = 0.1f;

        public void Awake()
        {

            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;
        }

        public void OnDestroy()
        {
             StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;
        }

        private void StationManager_OnRemoveUnit(UnitIndex unitIndex, StationIndex? stationRemovedUnitWasOn, BaseBattleUnit battleUnitOfDestroyedUnit)
        {

            /*  Animate the death of the Unit by simply shaking and phasing out the Unit.   */
            _ = UnitDeathPhaseOut(battleUnitOfDestroyedUnit);

            UnityEngine.Debug.LogError("Phase out animation done");
        }

        private async System.Threading.Tasks.Task UnitDeathPhaseOut(BaseBattleUnit battleUnitOfDestroyedUnit)
        {
            Vector3 unitPosition = battleUnitOfDestroyedUnit.gameObject.transform.position;
            if (unitPosition == null) { return; }

            float elapsedTime = 0;
            while (elapsedTime < PHASE_OUT_DURATION)
            {
                battleUnitOfDestroyedUnit.transform.position = new Vector3(Mathf.Sin(Time.time * PHASE_OUT_SHAKE_SPEED) * PHASE_OUT_DISTANCE + unitPosition.x, unitPosition.y, unitPosition.z);

                battleUnitOfDestroyedUnit.GetSpriteComponent().SetSpriteRenderTransparency(1 - (elapsedTime / PHASE_OUT_DURATION));

                elapsedTime += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }

            GameObject.Destroy(battleUnitOfDestroyedUnit.gameObject);
        }
    }
}