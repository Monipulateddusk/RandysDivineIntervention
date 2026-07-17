namespace TurnBased.Presentation
{
    public class AnimationUnitManager
    {
        private static AnimationUnitManager instance;
        public static AnimationUnitManager Instance
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

        private System.Collections.Generic.Dictionary<int, UnityEngine.Animator> UnitIndexAnimatorDict = new();

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            this.UnitIndexAnimatorDict = new();

            StationManager.OnAddUnit += StationManager_OnAddUnit;
            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }
            this.UnitIndexAnimatorDict.Clear();

            StationManager.OnAddUnit -= StationManager_OnAddUnit;
            StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;
        }

        private void StationManager_OnAddUnit(UnitIndex index)
        {
            AddUnitAnimatorToDictionary(index);
        }

        private void StationManager_OnRemoveUnit(UnitIndex index, StationIndex? nullable, BaseBattleUnit unit)
        {
            RemoveUnitAnimatorFromDictionary(index);
        }

        public bool AddUnitAnimatorToDictionary(UnitIndex unitIndex)
        {
            if (this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit unit)) {  return false; } 
            if (!unit.gameObject.TryGetComponent(out UnityEngine.Animator unitAnimator)) { return false; }

            UnityEngine.Debug.LogError("Added animator to dictionary of unitIndex: " + unitIndex.Index);

            this.UnitIndexAnimatorDict.Add(unitIndex.Index, unitAnimator);
            return true;
        }

        public bool RemoveUnitAnimatorFromDictionary(UnitIndex unitIndex)
        {
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexAnimatorDict.Remove(unitIndex.Index);
            return true;
        }

    }
}