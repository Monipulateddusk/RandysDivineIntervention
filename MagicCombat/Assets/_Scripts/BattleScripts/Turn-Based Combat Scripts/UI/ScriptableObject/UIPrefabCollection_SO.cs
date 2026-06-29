using UnityEngine;

[CreateAssetMenu(menuName="UI Database", fileName="UI Data")]
public class UICollection_SO : ScriptableObject
{
    [Header("UI Prefabs")]
    public GameObject   DialogueBoxPrefab;
    public GameObject   WindowMinimisationWidgetPrefab;
    public GameObject   CursorPrefab;
    public GameObject   OS_StartButtonPrefab;
    public GameObject   TaskbarHomeBoxPrefab;


    [Header("Scrollable UI")]
    public GameObject   ScrollableContentPrefab;
    public GameObject   ScrollableSlotPrefab;
    public GameObject   ScrollableItemPrefab;
    public Sprite       SummoningCircleSprite;

    [Header("Options UI")]
    public GameObject   OptionsMainPrefab;
    public GameObject   SliderElementPrefab;

    [Header("Inspection UI")]
    public GameObject   InspectionPrefab;
    public GameObject   InspectionHealthPrefab;

    [Header("Window Manager UI")]
    public GameObject   WindowManagerPrefab;

    [Header("Selector Manager UI")]
    public GameObject   SelectorManagerPrefab;

    [Header("VictoryLoss UI")]
    public GameObject   EndOfBattlePopupPrefab;
}
