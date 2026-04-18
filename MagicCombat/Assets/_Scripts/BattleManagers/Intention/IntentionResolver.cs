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

        private readonly Phases.MainTurnManager MainTurnManager = new();

        private System.Collections.Generic.Queue<UnitIndex> unitIndexesProcessingQueue;
        public static event System.Action OnAllIntentionsProcessed;

        public void Awake()
        {
            /*  Initalise the Singleton.    */
            instance = this;
        }

        public void SwitchSubPhase(MAIN_TURN_STATE newPhase) => this.MainTurnManager.SwitchSubPhase(newPhase);
        public void ContinueProcessIntention(UnitIndex unitIndex) => this.MainTurnManager.ContinueProcessIntention(unitIndex);

        /// <summary>
        /// Gets all Units on stations and if they implement a NON-PLAYER-DRIVEN MOVE-SELECTOR, then we process their intentions at the start of round.
        /// IMPORTANT: This should mean we auto-select move, but if the Unit has a PLAYER-DRIVEN MOVE-SELECTOR, then the PLAYER should be able to select the targets.
        /// </summary>
        public bool DetermineNonPlayerDrivenUnitIntentions()
        {
            this.unitIndexesProcessingQueue = new System.Collections.Generic.Queue<UnitIndex>(GetAllAutonomousUnits());      
            if (this.unitIndexesProcessingQueue.Count > 0)
            {
                ProcessNextUnitIndex();
                return true;
            }
            return false;
        }
        public bool DeterminePlayerDrivenUnitIntentions()
        {
            this.unitIndexesProcessingQueue = new System.Collections.Generic.Queue<UnitIndex>(GetAllPlayerDrivenUnits());
            if (this.unitIndexesProcessingQueue.Count > 0)
            {
                ProcessNextUnitIndex();
                return true;
            }
            return false;
        }

        public void ProcessNextUnitIndex()
        {
            UnityEngine.Debug.LogWarning($"Current Process Queue length is: {this.unitIndexesProcessingQueue.Count} ");

            if (this.unitIndexesProcessingQueue.Count == 0)
            {
                UnityEngine.Debug.LogWarning("ALL UNITS PROCESSED!");

                OnAllIntentionsProcessed?.Invoke();

                UnitIntentionManager.Instance.PrintOutAllIntents();
                return;
            }

            UnitIndex unitIndex = this.unitIndexesProcessingQueue.Dequeue();
            StationSelectorManager.Instance.SetSelectedStationIndex(unitIndex);
            ContinueProcessIntention(unitIndex);            
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

        public int GetQueueCount() => this.unitIndexesProcessingQueue.Count;
    }
}