using System.Collections.Generic;
using TurnBased;
using UnityEngine;

public struct UnitIntention
{
    public IBattleMoveAction MoveSelection;
    public List<int?> TargetIndexList;

    public UnitIntention(IBattleMoveAction moveData, List<int?> targetIndex)
    {
        this.MoveSelection = moveData;
        this.TargetIndexList = targetIndex;
    }
}


public class UnitIntentionManager : MonoBehaviour
{
    private static UnitIntentionManager instance;
    public static UnitIntentionManager Instance
    {
        get
        {
            try
            {
                return instance;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                return null;
            }
        }
    }

    private readonly Dictionary<int, UnitIntention> intentionDictionary = new();

    public static event System.Action<UnitIndex> OnUnitIntentionAdded;
    /// <summary>
    /// Invoked when a unit's intention changes due to move selection, target selection, switching out
    /// </summary>
    public static event System.Action<UnitIndex, UnitIntention> OnUnitIntentionChanged;
    public static event System.Action<UnitIndex> OnUnitIntentionRemoved;

    private void Awake()
    {
        InitaliseSingleton();
    }

    private void InitaliseSingleton()
    {
        /*  Initalise the Singleton.    */
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
        }
        instance = this;
    }

    public void AddUnitIndexToDictionary(UnitIndex unitIndex)
    {
        if (this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

        intentionDictionary.Add(unitIndex.Index, new());
        OnUnitIntentionAdded?.Invoke(unitIndex);
    }
    public void RemoveIntention(UnitIndex unitIndex)
    {
        if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

        intentionDictionary.Remove(unitIndex.Index);
        OnUnitIntentionRemoved?.Invoke(unitIndex);
    }

    public void SetIntention(UnitIndex unitIndex, UnitIntention intention)
    {
        if (!intentionDictionary.ContainsKey(unitIndex.Index)){ return;   }

        intentionDictionary[unitIndex.Index] = intention;
        OnUnitIntentionChanged?.Invoke(unitIndex, intention);
    }
    public void ClearIntention(UnitIndex unitIndex)
    {
        if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

        intentionDictionary[unitIndex.Index] = new();
        OnUnitIntentionChanged?.Invoke(unitIndex, new());
    }
    public bool TryGetIntention(UnitIndex unitIndex, out UnitIntention intention)
    {
        intention = default;
        if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return false; }

        intention = intentionDictionary[unitIndex.Index];
        return true;
    }
}
