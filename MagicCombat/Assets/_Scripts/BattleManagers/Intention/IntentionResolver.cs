using TurnBased.MoveSelection;
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
        public static event System.Action<UnitIndex, UnitIntentionResolutionState> OnUnitIntentionResolutionStateChange;
        public static event System.Action OnAllIntentionsProcessed;

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

        /// <summary>
        /// Gets all Units on stations and if they implement a NON-PLAYER-DRIVEN MOVE-SELECTOR, then we process their intentions at the start of round.
        /// IMPORTANT: This should mean we auto-select move, but if the Unit has a PLAYER-DRIVEN MOVE-SELECTOR, then the PLAYER should be able to select the targets.
        /// </summary>
        public void DetermineNonPlayerDrivenUnitIntentions()
        {
            this.unitIndexesProcessingQueue = new System.Collections.Generic.Queue<UnitIndex>(GetAllAutonomousUnits());

            ProcessNextUnitIndex();
        }   

        private void ProcessNextUnitIndex()
        {
            if(this.unitIndexesProcessingQueue.Count == 0)
            {
                Debug.LogWarning("ALL UNITS PROCESSED!");

                OnAllIntentionsProcessed?.Invoke();

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
                return; 
            }

            OnUnitIntentionResolutionStateChange?.Invoke(unitIndex, unitIntention.ResolutionState);

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
        private System.Collections.Generic.List<UnitIndex> GetAllAutonomousUnits()
        {
            System.Collections.Generic.List<UnitIndex> autonomousUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                if (!MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out IMoveSelector moveSelector)) { continue; }

                if (moveSelector is not PlayerDrivenMoveSelector)
                {
                    autonomousUnits.Add(unitIndex);
                }
            }
            return autonomousUnits; 
        }
    }
}