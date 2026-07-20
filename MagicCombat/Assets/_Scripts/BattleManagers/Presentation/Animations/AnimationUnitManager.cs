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

            this.UnitIndexAnimatorDict.Add(unitIndex.Index, unitAnimator);
            return true;
        }

        public bool RemoveUnitAnimatorFromDictionary(UnitIndex unitIndex)
        {
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexAnimatorDict.Remove(unitIndex.Index);
            return true;
        }


        public bool PlayAnimationForUnit(UnitIndex unitIndex, string stateName, out float animationDuration)
        {
            animationDuration = 0f;

            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            if (!DoesAnimationStateExistInUnitAnimator(this.UnitIndexAnimatorDict[unitIndex.Index], stateName)) {  return false; }

            this.UnitIndexAnimatorDict[unitIndex.Index].Play(stateName);
            animationDuration = this.UnitIndexAnimatorDict[unitIndex.Index].GetCurrentAnimatorClipInfo(0).Length;
            return true;
        }



        private bool DoesAnimationStateExistInUnitAnimator(UnityEngine.Animator animator, string stateName)
        {
            /*  As all animators are only on one layer (being the default, we just put this here)   */
            const int ANIMATION_LAYER_INDEX = 0;

            int stateID = UnityEngine.Animator.StringToHash(stateName);
            return animator.HasState(ANIMATION_LAYER_INDEX, stateID);
        }

    }
}