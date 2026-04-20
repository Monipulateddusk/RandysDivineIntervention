using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
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

        private bool intentionsComplete;
        public bool IsAllUnitIntentionsComplete
        {
            get
            {
                return intentionsComplete;
            }
            set
            {
                intentionsComplete = value;
            }
        }


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

            /*  If this selection change event is the currently selected unit, proceed to processing.   */
            if (this.currentActiveUnit.HasValue && this.currentActiveUnit.Value.Index == unitIndex.Index) 
            {
                ProcessSelectedUnitIntention(this.currentActiveUnit.Value);
            }
            /*  If it isn't, check to see if it is in our list. If it is, select that new unit. If not, don't override the selection.   */
            else
            {
                if (!DoesListContainUnitIndex(unitIndex)) {  return; }
                UnitSelectionForIntentionProcessing(unitIndex);
            }
        }


        public void SwitchSubPhase(MAIN_TURN_STATE newPhase) => this.MainTurnManager.SwitchSubPhase(newPhase);

        private void GetNextUnitInList(UnitIndex previousUnit)
        {
            if(DoesListContainUnitIndex(previousUnit) && this.currentActiveUnit.HasValue && this.currentActiveUnit.Value.Index == previousUnit.Index) 
            {
                if (!this.ProcessingUnitIndexes.Remove(previousUnit)){ UnityEngine.Debug.LogWarning($"ERROR — INTENTION MANAGER: UNABLE TO REMOVE THE PREVIOUS UNIT WHEN SELECTING NEW UNIT! UNIT DOES NOT EXIST IN COLLECTION"); }
            }

            /*  Deselect the UI.    */
            UI.UserInterfaceUserInput.Instance.StopSelection();

            /*  Retrieve the next new active unit if the container exists. If not, we are done. */
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                MonoBehaviour.print("<color=orange>Selecting new Unit in list</color>");

                /*  Select the UI, here and the station selector.   */
                this.currentActiveUnit = this.ProcessingUnitIndexes.FirstOrDefault();
                StationSelectorManager.Instance.SetSelectedStationIndex(this.currentActiveUnit.Value);
                UI.UserInterfaceUserInput.Instance.StartSelection(this.currentActiveUnit.Value);

                return;
            }

            /*  The list is empty, therefore we are done with our intentions.   */
            HandleIsDoneIntentions();
        }

        /// <summary>
        /// Gets all Units on stations and if they implement a NON-PLAYER-DRIVEN MOVE-SELECTOR, then we process their intentions at the start of round.
        /// IMPORTANT: This should mean we auto-select move, but if the Unit has a PLAYER-DRIVEN MOVE-SELECTOR, then the PLAYER should be able to select the targets.
        /// </summary>
        public bool DetermineNonPlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = GetAllAutonomousUnits();      
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                UnitSelectionForIntentionProcessing(this.ProcessingUnitIndexes.FirstOrDefault());
                return true;
            }
            return false;
        }
        public bool DeterminePlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = GetAllPlayerDrivenUnits();
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                UnitSelectionForIntentionProcessing(this.ProcessingUnitIndexes.FirstOrDefault());
                return true;
            }
            return false;
        }


        private void UnitSelectionForIntentionProcessing(UnitIndex unitIndex)
        {
            /*  Priority one. If this unit index doesn't exist in our Intention list, we don't want to process the intention and so we want to look into our list and get the first option from the list.   */
            if (!DoesListContainUnitIndex(unitIndex)){ GetNextUnitInList(unitIndex); }


            /*  If the currently selected unit is null. Set the selected unit to this new index.    */
            if (this.currentActiveUnit == null)
            {
                this.currentActiveUnit = unitIndex;
                StationSelectorManager.Instance.SetSelectedStationIndex(this.currentActiveUnit.Value);
                UI.UserInterfaceUserInput.Instance.StartSelection(this.currentActiveUnit.Value);
                return;
            }

            /*  If it isn't null, we want to peek at it. If it is NOT an identical UnitIndex, select this new unit to be this unit. */
            if (this.currentActiveUnit.Value.Index != unitIndex.Index)
            {
                this.currentActiveUnit = unitIndex;
                StationSelectorManager.Instance.SetSelectedStationIndex(this.currentActiveUnit.Value);
                UI.UserInterfaceUserInput.Instance.StartSelection(this.currentActiveUnit.Value);
                return;
            }
        }

        public void ContinueProcessingSelectedUnitIntention(UnitIndex unitIndex)
        {
            ProcessSelectedUnitIntention(unitIndex);
        }

        private void ProcessSelectedUnitIntention(UnitIndex unitIndex)
        {
            /*  Check the intention of the Unit here. If the unit is done. We move to the next Unit.    */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)){ return; }

            if (IsUnitIntentionDone(unitIndex, intention))
            {
                GetNextUnitInList(unitIndex);
                return;
            }
            OnUnitIntentionResolutionStateChange?.Invoke(this.currentActiveUnit.Value, intention.ResolutionState);
        }

        private void HandleIsDoneIntentions()
        {
            if (this.ProcessingUnitIndexes.Count == 0)
            {
                UnityEngine.Debug.LogWarning("ALL UNITS PROCESSED!");

                OnAllIntentionsProcessed?.Invoke();

                UnitIntentionManager.Instance.PrintOutAllIntents();
            }
        }

        private bool IsUnitIntentionDone(UnitIndex unitIndex, UnitIntention unitIntention)
        {
            if(unitIntention.ResolutionState == UnitIntentionResolutionState.COMPLETE) 
            { 
                return true; 
            }
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


        private bool DoesListContainUnitIndex(UnitIndex unitIndex)
        {
            int index = unitIndex.Index;

            foreach (UnitIndex uIndex in this.ProcessingUnitIndexes) 
            {
                if(uIndex.Index == index)
                {
                    return true;
                }
            }
            return false;
        }
    }
}