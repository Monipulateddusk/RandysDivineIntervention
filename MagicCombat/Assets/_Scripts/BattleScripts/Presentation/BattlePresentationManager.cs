using UnityEngine;

public class BattlePresentationManager : MonoBehaviour
{
    [SerializeField]GameObject tempVisual;
    private void Awake()
    {
        UnitSelectorManager.OnSelectionChange += UnitSelectorManager_OnSelectionChange;
    }

    private void UnitSelectorManager_OnSelectionChange(UnitSelectorManager.StationLocationData locationData)
    {
        if (tempVisual != null)
        {
            tempVisual.transform.position = new(locationData.Location.x, 0, locationData.Location.y);
        }

    }


}
