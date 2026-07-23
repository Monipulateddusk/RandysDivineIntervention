using System.Security.Cryptography.X509Certificates;

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
            if (!unit.GetAnimator()) { return false; }

            this.UnitIndexAnimatorDict.Add(unitIndex.Index, unit.GetAnimator());
            return true;
        }

        public bool RemoveUnitAnimatorFromDictionary(UnitIndex unitIndex)
        {
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexAnimatorDict.Remove(unitIndex.Index);
            return true;
        }


        public bool PlayAnimationForUnit(UnitIndex unitIndex, string stateName)
        {
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }
            if (!DoesAnimationStateExistInAnimator(this.UnitIndexAnimatorDict[unitIndex.Index], stateName)) {  return false; }

            this.UnitIndexAnimatorDict[unitIndex.Index].Play(stateName);
            return true;
        }

        public bool SetBooleanFlagForUnitAnimator(UnitIndex unitIndex, string boolFlagName, bool value)
        {
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }
            if (!DoesParameterExistInAnimator(this.UnitIndexAnimatorDict[unitIndex.Index], boolFlagName)) {  return false; }

            this.UnitIndexAnimatorDict[unitIndex.Index].SetBool(boolFlagName, value);
            return true;
        }

        public bool GetAnimationDurationForUnitAnimator(UnitIndex unitIndex, out float animationDuration)
        {
            animationDuration = 0f;
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            animationDuration = this.UnitIndexAnimatorDict[unitIndex.Index].GetCurrentAnimatorClipInfo(0).Length;
            return true;
        }

        public bool SetTriggerFlagForUnitAnimator(UnitIndex unitIndex, string triggerFlagName)
        {
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }
            if (!DoesParameterExistInAnimator(this.UnitIndexAnimatorDict[unitIndex.Index], triggerFlagName)) { return false; }

            this.UnitIndexAnimatorDict[unitIndex.Index].ResetTrigger(triggerFlagName);
            this.UnitIndexAnimatorDict[unitIndex.Index].SetTrigger(triggerFlagName);

            return true;
        }

        public bool GetDurationToNextAnimationEvent(UnitIndex unitIndex, out float animationEventDuration)
        {
            animationEventDuration = 0;
            if (!this.UnitIndexAnimatorDict.ContainsKey(unitIndex.Index)) { return false; }

            /*  As all animators are only on one layer (being the default, we just put this here)   */
            const int ANIMATION_LAYER_INDEX = 0;

            UnityEngine.AnimationEvent[] animationEvents = this.UnitIndexAnimatorDict[unitIndex.Index].GetCurrentAnimatorClipInfo(ANIMATION_LAYER_INDEX)[0].clip.events;
            if (animationEvents.Length <= 0) { return false; }

            /*  As currently we only have one animation event to determine where the 'Swing' of an attack is, get the 0 index animation event.  */
            animationEventDuration = animationEvents[0].time;
            return true;
        }

        private bool DoesParameterExistInAnimator(UnityEngine.Animator animator, string paramName)
        {
            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.parameters[i].name == paramName) {  return true; }
            }
            return false;
        }

        private bool DoesAnimationStateExistInAnimator(UnityEngine.Animator animator, string stateName)
        {
            /*  As all animators are only on one layer (being the default, we just put this here)   */
            const int ANIMATION_LAYER_INDEX = 0;

            int stateID = UnityEngine.Animator.StringToHash(stateName);
            return animator.HasState(ANIMATION_LAYER_INDEX, stateID);
        }

    }
}