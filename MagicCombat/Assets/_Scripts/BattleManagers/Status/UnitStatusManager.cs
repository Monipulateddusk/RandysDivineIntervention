namespace TurnBased.Status
{
    public class UnitStatus
    {

    }

    public class UnitStatusManager
    {
        private static UnitStatusManager instance;
        public static UnitStatusManager Instance
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

        private System.Collections.Generic.Dictionary<int, UnitStatus> intentionDictionary = new();


        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.intentionDictionary = new();

            StationManager.OnAddUnit += AddUnitIndexToDictionary;
            StationManager.OnRemoveUnit += RemoveUnitIndexFromDictionary;
        }



        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.intentionDictionary.Clear();

            StationManager.OnAddUnit -= AddUnitIndexToDictionary;
            StationManager.OnRemoveUnit -= RemoveUnitIndexFromDictionary;
        }

        public void AddUnitIndexToDictionary(UnitIndex unitIndex)
        {
            if (this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary.Add(unitIndex.Index, new());
        }

        private void RemoveUnitIndexFromDictionary(UnitIndex unitIndex, StationIndex? arg2, BaseBattleUnit arg3)
        {
            UnityEngine.Debug.LogError("Starting to remove intention from UnitIntentionManager");

            RemoveIntention(unitIndex);

            UnityEngine.Debug.LogError("Removed unit index from UnitIntentionManager");
        }

        public void RemoveIntention(UnitIndex unitIndex)
        {
            if (!this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary.Remove(unitIndex.Index);
        }
    }
}