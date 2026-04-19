using System.Diagnostics;
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
                    UnityEngine.Debug.LogError(e.ToString());
                    return null;
                }
            }
        }

        public static event System.Action<UnitIndex, UnitIntentionResolutionState> OnUnitIntentionResolutionStateChange;

        private readonly Phases.MainTurnManager MainTurnManager = new();

        private System.Collections.Generic.List<UnitIndex> ProcessingUnitIndexes;
        public static event System.Action OnAllIntentionsProcessed;
        private UnitIndex? currentActiveUnit;



        public void Awake()
        {
            /*  Initalise the Singleton.    */
            instance = this;

            StationSelectorManager.OnSelectionChange += StationSelectorManager_OnSelectionChange;
        }

        public void OnDestroy()
        {
            StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex newSelectedStation, StationIndex? oldStation)
        {
            /*  Get the selected Unit's unit Index  */
            if (!StationManager.Instance.TryGetUnitIndexOnStation(newSelectedStation, out UnitIndex unitIndex)) { return; }

            ProcessUnit(unitIndex);
        }


        public void SwitchSubPhase(MAIN_TURN_STATE newPhase) => this.MainTurnManager.SwitchSubPhase(newPhase);

        /// <summary>
        /// Gets all Units on stations and if they implement a NON-PLAYER-DRIVEN MOVE-SELECTOR, then we process their intentions at the start of round.
        /// IMPORTANT: This should mean we auto-select move, but if the Unit has a PLAYER-DRIVEN MOVE-SELECTOR, then the PLAYER should be able to select the targets.
        /// </summary>
        public bool DetermineNonPlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = GetAllAutonomousUnits();      
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                ProcessUnit(this.ProcessingUnitIndexes.FirstOrDefault());
                return true;
            }
            return false;
        }
        public bool DeterminePlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = GetAllPlayerDrivenUnits();
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                ProcessUnit(this.ProcessingUnitIndexes.FirstOrDefault());
                return true;
            }
            return false;
        }

        public bool ProcessUnit(UnitIndex unitToProcess)
        {
            /*  If the new Unit to process exists in our processing list. We want to proceed to selection.  */
            if (this.ProcessingUnitIndexes.Contains(unitToProcess))
            {
                if (this.currentActiveUnit.HasValue && this.currentActiveUnit.Value.Index == unitToProcess.Index)
                {
                    return false;
                }
                MonoBehaviour.print($"WHOOGA WOGOOS");

                SelectUnit(unitToProcess);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Our Main entry point for our resolver
        /// </summary>
        /// <param name="selectedUnitIndex"></param>
        private void SelectUnit(UnitIndex selectedUnitIndex)
        {
            /*  When we select a new unit, deselect and stop processes for the previously selected unit.    */
            if (this.currentActiveUnit.HasValue)
            {
                UnityEngine.Debug.LogWarning($"Cancelling previous Unit's processes with UnitIndex: {this.currentActiveUnit.Value.Index} for new selected Unit of: {selectedUnitIndex.Index}");
            }
            UI.UserInterfaceUserInput.Instance.StopSelection();


            /*  Check the intention of this Unit. If the intention is done, don't select this Unit. */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(selectedUnitIndex, out Intention.UnitIntention intention)) { return; }
            if (IsUnitIntentionDone(selectedUnitIndex, intention)) { return;   }

            this.currentActiveUnit = selectedUnitIndex;
            StationSelectorManager.Instance.SetSelectedStationIndex(this.currentActiveUnit.Value);

            /*  Once we have selected a Unit, process it.   */
            ProcessSelectedIndex(intention);
        }


        private void ProcessSelectedIndex(Intention.UnitIntention intention)
        {
            UnityEngine.Debug.LogWarning($"Current Process Queue length is: {this.ProcessingUnitIndexes.Count} ");

            /*  If the list is complete, invoke the event.  */
            if (HandleIsDoneIntentions()) { return; }

            OnUnitIntentionResolutionStateChange?.Invoke(this.currentActiveUnit.Value, intention.ResolutionState);

            /*  
             *  When the intention changes (I.e. when a move OR target is done selecting), jump to the Idle Phase.  
             */
            SwitchSubPhase(MAIN_TURN_STATE.IDLE);

        }

        private bool HandleIsDoneIntentions()
        {
            if (this.ProcessingUnitIndexes.Count == 0)
            {
                UnityEngine.Debug.LogWarning("ALL UNITS PROCESSED!");

                OnAllIntentionsProcessed?.Invoke();

                UnitIntentionManager.Instance.PrintOutAllIntents();
                return true;
            }
            return false;
        }

        private bool IsUnitIntentionDone(UnitIndex unitIndex, Intention.UnitIntention intention)
        {
            if(intention.ResolutionState == UnitIntentionResolutionState.COMPLETE) { return true; }
            return false;
        }


        private System.Collections.Generic.List<UnitIndex> GetAllAutonomousUnits()
        {
            System.Collections.Generic.List<UnitIndex> autonomousUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { continue; }

                if (moveSelector is not MoveSelection.PlayerDrivenMoveSelector)
                {
                    autonomousUnits.Add(unitIndex);
                }
            }
            return autonomousUnits; 
        }

        private System.Collections.Generic.List<UnitIndex> GetAllPlayerDrivenUnits()
        {
            System.Collections.Generic.List<UnitIndex> playerDrivenUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { continue; }

                if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
                {
                    playerDrivenUnits.Add(unitIndex);
                }
            }
            return playerDrivenUnits;
        }


        public int GetQueueCount() => this.ProcessingUnitIndexes.Count;
    }
}