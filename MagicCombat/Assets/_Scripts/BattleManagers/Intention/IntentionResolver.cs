using System.Linq;
using UnityEngine;

namespace TurnBased.Intention
{
    public class IntentionResolver
    {
        private static IntentionResolver instance;
        public static IntentionResolver Instance
        {
            get
            {
                try
                {
                    return instance;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                    return null;
                }
            }
        }

        private System.Collections.Generic.List<UnitIndex> unitIndexesProcessing;
        private UnitIndex unitIndexCurrentlyProcessing;

        public void Awake()
        {
            /*  Initalise the Singleton.    */
            instance = this;

            MoveSelectionResolver.OnMoveSelectionComplete       += MoveSelectionResolver_OnMoveSelectionComplete;
            TargetSelectionResolver.OnTargetSelectionComplete   += TargetSelectionResolver_OnTargetSelectionComplete;
        }

        ~IntentionResolver()
        {
            MoveSelectionResolver.OnMoveSelectionComplete       -= MoveSelectionResolver_OnMoveSelectionComplete;
            TargetSelectionResolver.OnTargetSelectionComplete   -= TargetSelectionResolver_OnTargetSelectionComplete;
        }

        private void MoveSelectionResolver_OnMoveSelectionComplete(UnitIndex unitIndex)
        {
            ProcessIntention(unitIndex);
        }

        private void TargetSelectionResolver_OnTargetSelectionComplete(UnitIndex unitIndex)
        {
            /*  This unit is done selecting it's target. Remove this unitIndex from the list and move onto the next one.    */
            this.unitIndexesProcessing.Remove(unitIndex);

            this.unitIndexCurrentlyProcessing = this.unitIndexesProcessing.FirstOrDefault();

            ProcessIntention(this.unitIndexCurrentlyProcessing);
        }

        public void DetermineEnemyUnitIntentions()
        {
            Debug.Log("Starting enemy intent");

            if (!StationManager.Instance.TryGetUnitIndexesOfTeam(UnitTeam.ENEMY, out System.Collections.Generic.List<UnitIndex> enemyUnitIndexes)) { return; }


            this.unitIndexesProcessing = enemyUnitIndexes;
            this.unitIndexCurrentlyProcessing = this.unitIndexesProcessing.FirstOrDefault();

            Debug.Log("Processing intention for unit index: " + this.unitIndexCurrentlyProcessing);

            ProcessIntention(this.unitIndexCurrentlyProcessing);
        }        

        private void ProcessIntention(UnitIndex unitIndex)
        {
            /*  Retrieve the Unit Intention for this Unit to determine which phase of the Intention we are. */
            if (!UnitIntentionManager.Instance.TryGetIntention(unitIndex, out UnitIntention unitIntention)) { return; }

            switch (unitIntention.ResolutionState)
            {
                case UnitIntentionResolutionState.NONE:
                    return;
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    MoveSelectionResolver.ProcessIntentionMoveSelection(unitIndex);
                    return;
                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    TargetSelectionResolver.ProcessIntentionTargetSelection(unitIndex);
                    return;
                case UnitIntentionResolutionState.COMPLETE:

                    return;
            }
        }
    }
}