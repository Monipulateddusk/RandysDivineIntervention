using UnityEngine;

namespace TurnBased.UI
{
    public class UnitHealthDisplayUI : MonoBehaviour
    {
        [SerializeField] GameObject mainHealthBarGameObject;
        [SerializeField] GameObject healthBarValueStringGameObject;
        [SerializeField] UnityEngine.UI.LayoutElement layoutElementComponent;

        private TMPro.TextMeshProUGUI healthValueTMP;
        UnitIndex associatedUnitIndex;



        public void Initalise(UnitIndex indexOfUnitHealthCorrelatesTo)
        {
            this.associatedUnitIndex = indexOfUnitHealthCorrelatesTo;
            InitaliseHealthTextTMP();

            Health.UnitHealthManager.OnUnitHealthChange += UnitHealthManager_OnUnitHealthChange;


            /*  Once everything is initalised, update the health bar for the current health values. */
            UpdateHealthAmount();
        }

        private void UnitHealthManager_OnUnitHealthChange(UnitIndex unitIndexWhoseHealthChanged, int newHealthValueOfUnitIndex)
        {
            /*  If health changed on a Unit we aren't associated with, don't do anything.   */
            if(unitIndexWhoseHealthChanged.Index != this.associatedUnitIndex.Index) { return; }

            UpdateHealthAmount();
        }

        private void OnDestroy()
        {
            Health.UnitHealthManager.OnUnitHealthChange -= UnitHealthManager_OnUnitHealthChange;
        }

        private void UpdateHealthAmount()
        {
            if (this.mainHealthBarGameObject != null && this.healthBarValueStringGameObject != null)
            {
                /*  Get the battle unit of this unit to get it's current Health and max health. */
                if (!Health.UnitHealthManager.Instance.GetCurrentHealthOfUnitIndex(this.associatedUnitIndex, out int currentHealth)) { return; }
                if (!Health.UnitHealthManager.Instance.GetMaximumHealthOfUnitIndex(this.associatedUnitIndex, out int maximumHealth)) { return; }

                float value = UserInterfaceUtility.GetValueNormalisation(minimum: 0, maximum: maximumHealth, current: currentHealth);

                this.mainHealthBarGameObject.transform.localScale = new(value, 1, 1);
                this.healthValueTMP.text = currentHealth.ToString() + "/" + maximumHealth.ToString();
            }
        }

        private void InitaliseHealthTextTMP()
        {
            if(this.healthBarValueStringGameObject != null)
            {
                if(this.healthBarValueStringGameObject.TryGetComponent(out TMPro.TextMeshProUGUI tMP)){
                    this.healthValueTMP = tMP;
                }
            }
        }

    }
}