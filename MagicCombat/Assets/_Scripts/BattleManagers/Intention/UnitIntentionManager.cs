using System.Linq;
using UnityEngine;

namespace TurnBased.Intention
{
    public class UnitIntention
    {
        public IBattleMove MoveSelection { get; }
        public System.Collections.Generic.List<StationIndex> TargetIndexList { get; }
        public UnitIntentionResolutionState ResolutionState { get; }

        public UnitIntention(IBattleMove moveData, System.Collections.Generic.List<StationIndex> targetIndex)
        {
            this.MoveSelection = moveData;
            this.TargetIndexList = targetIndex;
            this.ResolutionState = UnitIntentionResolutionState.COMPLETE;
        }

        public UnitIntention(IBattleMove moveData)
        {
            this.MoveSelection = moveData;
            this.TargetIndexList = new();
            this.ResolutionState = UnitIntentionResolutionState.AWAITING_TARGET_SELECTION;
        }
        public UnitIntention()
        {
            this.MoveSelection = null;
            this.TargetIndexList = new();
            this.ResolutionState = UnitIntentionResolutionState.AWAITING_MOVE_SELECTION;
        }
    }


    public class UnitIntentionManager
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

        private readonly System.Collections.Generic.Dictionary<int, UnitIntention> intentionDictionary = new();

        public static event System.Action<UnitIndex> OnUnitIntentionAdded;
        /// <summary>
        /// Invoked when a unit's intention changes due to move selection, target selection, switching out
        /// </summary>
        public static event System.Action<UnitIndex, UnitIntention> OnUnitIntentionChanged;
        public static event System.Action<UnitIndex> OnUnitIntentionRemoved;

        public void Awake()
        {
            instance = this;
            StationManager.OnAddUnit += AddUnitIndexToDictionary;
        }

        ~UnitIntentionManager()
        {
            StationManager.OnAddUnit -= AddUnitIndexToDictionary;
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
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary[unitIndex.Index] = intention;
            OnUnitIntentionChanged?.Invoke(unitIndex, intention);
        }

        public void SetMoveIntention(UnitIndex unitIndex, IBattleMove battleMove)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary[unitIndex.Index] = new(battleMove);
            OnUnitIntentionChanged?.Invoke(unitIndex, intentionDictionary[unitIndex.Index]);

            Debug.Log("Setting Move Intention for Unit Index: " + unitIndex.Index + " Move Intention: " +  battleMove.GetMoveName()); 

        }

        public void SetTargetIntention(UnitIndex unitIndex, System.Collections.Generic.List<StationIndex> targetIntentionList)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            UnitIntention currentIntention = intentionDictionary[unitIndex.Index];

            this.intentionDictionary[unitIndex.Index] = new(currentIntention.MoveSelection, targetIntentionList);
            OnUnitIntentionChanged?.Invoke(unitIndex, intentionDictionary[unitIndex.Index]);

            Debug.Log("Setting target intention for Unit Index: " + unitIndex.Index + " target intention size is: " + targetIntentionList.Count);
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

        public void PrintOutAllIntents()
        {
            UnityEngine.Debug.LogError("Printing all intents!");
            foreach (var index in this.intentionDictionary)
            {
                UnityEngine.Debug.LogError($"Unit Index: {index.Key} has selected: {index.Value.MoveSelection} and selected Station {index.Value.TargetIndexList.FirstOrDefault().Index} as their target ");
            }
        }
    }
}