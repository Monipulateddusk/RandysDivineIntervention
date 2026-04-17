using System.Linq;
using UnityEngine;
namespace TurnBased.Intention {
    public class IntentionVisualiserManager
    {
        GameObject textPrefab;

        public void Awake()
        {
            UnitIntentionManager.OnUnitIntentionChanged += UnitIntentionManager_OnUnitIntentionChanged;
        }

        ~IntentionVisualiserManager()
        {
            UnitIntentionManager.OnUnitIntentionChanged -= UnitIntentionManager_OnUnitIntentionChanged;
        }
        private void UnitIntentionManager_OnUnitIntentionChanged(UnitIndex unitIndex, UnitIntention intentionOfUnit)
        {
            /*  Get the base battle unit for us to add a game object on to. */
            if(!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit battleUnit)) { return; }

            /*  Check to see if there is already a child. If so, dont create a new one  */
            TMPro.TextMeshPro textElement = battleUnit.gameObject.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textElement == null)
            {
                GameObject objectDisplay = CreateIntentionGameObjectDisplay(battleUnit.gameObject);
                textElement = objectDisplay.GetComponent<TMPro.TextMeshPro>();
                textElement.text = GetIntentionText(battleUnit, intentionOfUnit);
            }
            else
            {
                textElement.text = GetIntentionText(battleUnit, intentionOfUnit);
            }



        }

        private string GetIntentionText(BaseBattleUnit battleUnit, UnitIntention intentionOfUnit)
        {
            /*  Get the unit name of  the target.   */
            if (!StationManager.Instance.TryGetUnitDataOnStation(intentionOfUnit.TargetIndexList.FirstOrDefault(), out UnitData unitData)) { return string.Empty; }

            return $"{battleUnit.GetBaseUnit().name} is using a {intentionOfUnit.MoveSelection.GetMoveName()} on: {unitData.name}";
        }

        private GameObject CreateIntentionGameObjectDisplay(GameObject parentGameObject)
        {
            if (this.textPrefab == null) { return new GameObject();  }
            return GameObject.Instantiate(textPrefab, parentGameObject.transform); 
        }


        public void SetTextPrefab(GameObject textPrefab) {  this.textPrefab = textPrefab;}
    }
}