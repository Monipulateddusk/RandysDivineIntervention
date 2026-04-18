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

            /*  Once everything is initalised, update the health bar for the current health values. */
            UpdateHealthAmount();
        }

        private void UpdateHealthAmount()
        {
            if (this.mainHealthBarGameObject != null && this.healthBarValueStringGameObject != null)
            {
                /*  Get the battle unit of this unit to get it's current Health and max health. */
                if(!StationManager.Instance.TryGetBattleUnitOfIndex(this.associatedUnitIndex, out BaseBattleUnit battleUnit)) { return; }

                int currentHealth = battleUnit.GetHealthComponent().GetHealth();
                int maximumHealth = battleUnit.GetBaseUnit().maxHP;

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