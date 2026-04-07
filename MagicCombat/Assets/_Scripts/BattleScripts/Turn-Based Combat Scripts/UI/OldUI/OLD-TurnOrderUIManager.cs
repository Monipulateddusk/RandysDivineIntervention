using System.Collections.Generic;
using TurnBased;
using UnityEditor;
using UnityEngine;

public interface IBattleUI
{
    void Update();
}

public class TurnOrderUIManager : MonoBehaviour
{
    [SerializeField] GameObject currentUnitSlot, otherUnitSlots;
    private GameObject backgroundTemplate = null, iconTemplate;
    private readonly List<GameObject> Icons = new();

    const float MIN_SLOTS_SIZE = 30, SLOT_SIZE_INCREMENT_AMMOUNT = -65.0f;
    const uint   MIN_SLOTS_COUNT = 1, MAX_SLOTS_COUNT = 10;
    private void Awake()
    {
        CreateBackgroundTemplate();
        CreateIconTemplate();

        BattleMediator.OnUpdateTurnOrder += OnUpdateTurnOrderList;
    }

    private void OnDestroy()
    {
        BattleMediator.OnUpdateTurnOrder -= OnUpdateTurnOrderList;
    }

    private void CreateBackgroundTemplate()
    {
        if (backgroundTemplate == null)
        {
            backgroundTemplate = new GameObject("Background");
            backgroundTemplate.AddComponent<CanvasRenderer>();
            backgroundTemplate.AddComponent<RectTransform>();
            backgroundTemplate.AddComponent<UnityEngine.UI.Image>();
            backgroundTemplate.AddComponent<UnityEngine.UI.Outline>();

            //Destroy(backgroundTemplate);
        }
    }

    private void CreateIconTemplate()
    {
        if(iconTemplate == null)
        {
            iconTemplate = new GameObject("Icon");
            iconTemplate.AddComponent<CanvasRenderer>();
            iconTemplate.AddComponent<RectTransform>();
            iconTemplate.AddComponent <UnityEngine.UI.Image>();

            //Destroy(iconTemplate);
        }
    }

    private void SetInstanceObjectParent(ref GameObject globalParent, ref GameObject child, ref GameObject subChild)
    {
        subChild.transform.SetParent(child.transform, false);
        child.transform.SetParent(globalParent.transform, false);
    }

    private void SetImageProperties(ref UnityEngine.UI.Image image, ref Sprite sprite, Color color)
    {
        image.color = color;
        image.sprite = sprite;
    }
    private void SetImageProperties(ref UnityEngine.UI.Image image, Color color)
    {
        image.color = color;
    }

    private void SetOutlineProperties(ref UnityEngine.UI.Outline outline, Color color, Vector2 effectDist)
    {
        outline.effectColor = color;
        outline.effectDistance = new Vector2(3, -3);
    }

    private void SetRectTransformProperties(ref RectTransform transform)
    {
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = new Vector2(1.0f, 1.0f);
        transform.offsetMin = Vector2.zero;
        transform.offsetMax = Vector2.zero;
    }
    private void SetRectTransformProperties(ref RectTransform transform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    { 
        transform.anchorMin = anchorMin;
        transform.anchorMax = anchorMax; // ;
        transform.offsetMin = offsetMin; // Vector2.zero;
        transform.offsetMax = offsetMax;
    }

    private void CreateChildUnitTurnIconObject(UnitIndex unitIndex, GameObject parentGameObject)
    {
        /*  Retrieve the information about that Unit.   */
        if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit battleUnit)) { return; }
        UnitTeam team = StationManager.Instance.GetUnitTeamOfIndex(unitIndex);

        /*  Create the UI Child Object that will go into the Horizontal Group.  */
        GameObject childBackground = Instantiate(backgroundTemplate);
        GameObject subChildIcon = Instantiate(iconTemplate);

        SetInstanceObjectParent(ref parentGameObject, ref childBackground, ref subChildIcon);

        UnityEngine.RectTransform childBackgroundTransform = childBackground.GetComponent<UnityEngine.RectTransform>();
        SetRectTransformProperties(ref childBackgroundTransform);
        
        /*  What Icon are we using? */
        UnityEngine.UI.Image objImage = childBackground.GetComponent<UnityEngine.UI.Image>();
        SetImageProperties(ref objImage, new Color(0.7f, 0.7f, 0.7f));

        /*  Are we an Ally or Enemy?    */
        UnityEngine.UI.Outline outline = childBackground.GetComponent<UnityEngine.UI.Outline>();
        SetOutlineProperties(ref outline, ( team == UnitTeam.ALLY ? Color.green : Color.red ), new Vector2(3, -3));

        /*  Implement the Icon based on the Unit.   */
        UnityEngine.UI.Image subChildIconImage = subChildIcon.GetComponent<UnityEngine.UI.Image>();
        SetImageProperties(ref subChildIconImage, ref battleUnit.GetBaseUnit().sprite, battleUnit.GetBaseUnit().color);

        UnityEngine.RectTransform subChildIconTransform = subChildIcon.GetComponent<UnityEngine.RectTransform>();
        SetRectTransformProperties(ref subChildIconTransform);

        Icons.Add(childBackground);
    }

    private void UpdateOtherUnitSlotsSize(List<UnitIndex> obj)
    {
        if (otherUnitSlots != null)
        {
            RectTransform transform = otherUnitSlots.GetComponent<RectTransform>();
            float left = MIN_SLOTS_SIZE;

            /*  Get the length of the List<UnitIndex> List */
            int size = obj.Count - 1;

            left = (size >= 0) ? left += SLOT_SIZE_INCREMENT_AMMOUNT * size : 100;

            SetRectTransformProperties(ref transform, Vector2.zero, new Vector2(1, 1), new Vector2(left, 0), Vector2.zero);
        }
    }

    private void UpdateCurrentUnitSlotPosition(List<UnitIndex> obj)
    {
        if (currentUnitSlot != null)
        {
            RectTransform transform = currentUnitSlot.GetComponent<RectTransform>();

            float left = -70; // -70 is the starting size of CurrentUnitSlot if there is at least 1 otherUnitSlot 

            /*  Get the length of the List<UnitIndex> List */
            int size = obj.Count - 1;

            left = (size >= 0) ? left += SLOT_SIZE_INCREMENT_AMMOUNT * size : 0;

            SetRectTransformProperties(ref transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(left, 0), new Vector2(+left, 0));
        }
        //-680
    }
    private void DeleteIcons()
    {
        foreach(GameObject obj in Icons)
        {
            DestroyImmediate(obj);
        }
    }

    private void OnUpdateTurnOrderList(List<UnitIndex> obj)
    {
        DeleteIcons();

        /*  Push the Current Unit into the Icons List.  */
        CreateChildUnitTurnIconObject(BattleMediator.Instance.GetCurrentUnit().Value, currentUnitSlot);

        UpdateCurrentUnitSlotPosition(obj);
        UpdateOtherUnitSlotsSize(obj);

        foreach (UnitIndex index in obj)
        {
            CreateChildUnitTurnIconObject(index, otherUnitSlots);    
        }
    }


}
