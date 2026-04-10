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

        private System.Collections.Generic.Queue<UnitIndex> unitIndexesProcessingQueue;

        public void Awake()
        {
            /*  Initalise the Singleton.    */
            instance = this;

            MoveSelectionResolver.OnMoveSelectionComplete       += ResumeProcessingIntention;
            TargetSelectionResolver.OnTargetSelectionComplete   += ResumeProcessingIntention;
        }

        ~IntentionResolver()
        {
            MoveSelectionResolver.OnMoveSelectionComplete       -= ResumeProcessingIntention;
            TargetSelectionResolver.OnTargetSelectionComplete   -= ResumeProcessingIntention;
        }

        public void ResumeProcessingIntention(UnitIndex unitIndex)
        {
            ProcessIntention(unitIndex);
        }
       

        public void DetermineEnemyUnitIntentions()
        {
            Debug.Log("Starting enemy intent");

            if (!StationManager.Instance.TryGetUnitIndexesOfTeam(UnitTeam.ENEMY, out System.Collections.Generic.List<UnitIndex> enemyUnitIndexes)) { return; }
            Debug.Log("Got indexes of team for purposes of IntentionResolver.   ");

            unitIndexesProcessingQueue = new System.Collections.Generic.Queue<UnitIndex>(enemyUnitIndexes);

            ProcessNextUnitIndex();
        }        

        private void ProcessNextUnitIndex()
        {
            if(this.unitIndexesProcessingQueue.Count == 0)
            {
                Debug.LogWarning("ALL UNITS PROCESSED!");

                UnitIntentionManager.Instance.PrintOutAllIntents();
                return;
            }

            UnitIndex unitIndex = this.unitIndexesProcessingQueue.Dequeue();    
            ProcessIntention(unitIndex);
        }

        private void ProcessIntention(UnitIndex unitIndex)
        {
            /*  Retrieve the Unit Intention for this Unit to determine which phase of the Intention we are. */
            if (!UnitIntentionManager.Instance.TryGetIntention(unitIndex, out UnitIntention unitIntention)) 
            {
                Debug.Log("There is no intention for this unit at index: " + unitIndex.Index + " Creating one!");
                UnitIntentionManager.Instance.AddUnitIndexToDictionary(unitIndex);
                
                if(!UnitIntentionManager.Instance.TryGetIntention(unitIndex, out unitIntention)) { UnityEngine.Debug.Log("Idk bro, this is just cursed."); return; }
            }

            switch (unitIntention.ResolutionState)
            {
                case UnitIntentionResolutionState.NONE:
                    UnityEngine.Debug.Log("No state?");

                    return;
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    UnityEngine.Debug.Log("Selecting move");
                    MoveSelectionResolver.ProcessIntentionMoveSelection(unitIndex);
                    return;
                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    UnityEngine.Debug.Log("Selecting target");

                    TargetSelectionResolver.ProcessIntentionTargetSelection(unitIndex);
                    return;
                case UnitIntentionResolutionState.COMPLETE:
                    UnityEngine.Debug.LogWarning("Complete?");
                    ProcessNextUnitIndex();
                    return;
            }
        }
    }
}