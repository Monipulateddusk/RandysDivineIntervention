using UnityEngine;
namespace TurnBased.Intention {
    public class IntentionVisualiserManager
    {

        public void Awake()
        {
           // UnitIntentionManager.OnUnitIntentionChanged += UnitIntentionManager_OnUnitIntentionChanged;
        }

        public void OnDestroy()
        {
            //UnitIntentionManager.OnUnitIntentionChanged -= UnitIntentionManager_OnUnitIntentionChanged;
        }

        private void UnitIntentionManager_OnUnitIntentionChanged(UnitIndex unitIndex, UnitIntention intentionOfUnit)
        {
        }
        
    }
}