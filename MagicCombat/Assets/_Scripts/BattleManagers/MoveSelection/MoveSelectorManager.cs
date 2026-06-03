using System.Linq;
using UnityEngine;

namespace TurnBased.MoveSelection {
    public class MoveSelectorManager
    {
        private static MoveSelectorManager instance;
        public static MoveSelectorManager Instance
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

        private System.Collections.Generic.Dictionary<int, IMoveSelector> unitIndexMoveSelectionDictionary = new();

        public void Awake()
        {
            /*  Initalise the Singleton.    */
            if (instance == null)
            {
                instance = this;
            }

            this.unitIndexMoveSelectionDictionary = new();

            StationManager.OnAddUnit    += StationManager_OnAddUnit;
            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;

            Debug.LogWarning($"Subscribed to Station Manager add unit!");


        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            StationManager.OnAddUnit    -= StationManager_OnAddUnit;
            StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;
            RemoveAllUnitIndexesFromDictionary();
        }

        private void StationManager_OnAddUnit(UnitIndex unitIndex)
        {
            UnityEngine.Debug.LogError($"Trying to get battle unit of index");

            if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit unit)) { return; }

            UnityEngine.Debug.LogError($"Adding unit to move selector   ");
            AddUnitMoveSelector(unitIndex, unit.GetBaseUnit().moveSelectorType);
        }
        private void StationManager_OnRemoveUnit(UnitIndex unitIndex, StationIndex? nullable, BaseBattleUnit unit)
        {
            Debug.LogError("Starting to remove unit from MoveSelectorManager");

            RemoveUnitMoveSelector(unitIndex);

            Debug.LogError("Removed unit from MoveSelectorManager");
        }

        public bool AddUnitMoveSelector(UnitIndex unitIndex, UnitMoveSelectorType moveSelectorType)
        {
            /*  If we already have this index, stop!    */
            if(this.unitIndexMoveSelectionDictionary.ContainsKey(unitIndex.Index)) {return false;}

            /*  When called, retrieve the IMoveSelector type.   */
            if (!MoveSelectorHandler.TryGetSelector(moveSelectorType, out IMoveSelector moveSelector)) { return false; }

            /*  If it is an instanciatable selector.    */
            if (MoveSelectorHandler.IsSelectorTypeInstanciatable(moveSelectorType))
            {
                /* Create a new instance of the class. Important for selectors like Sequential which have a unit-driven 'memory' for the previously selected move.  */
                IMoveSelector newSelectorInstance = (IMoveSelector)System.Activator.CreateInstance(moveSelector.GetType());

                this.unitIndexMoveSelectionDictionary.Add(unitIndex.Index, newSelectorInstance);
            }
            /*  If not, just get a referance to this Class. */
            else
            {
                this.unitIndexMoveSelectionDictionary.Add(unitIndex.Index, moveSelector);
            }

            return true;
        }

        public bool RemoveUnitMoveSelector(UnitIndex unitIndex)
        {
            /*  If we don't have this index, stop!    */
            if (!this.unitIndexMoveSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }
            
            this.unitIndexMoveSelectionDictionary.Remove(unitIndex.Index);
            return true;        
        }
        private void RemoveAllUnitIndexesFromDictionary()
        {
            System.Collections.Generic.List<int> keys = this.unitIndexMoveSelectionDictionary.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                this.unitIndexMoveSelectionDictionary.Remove(keys[i]);
            }
        }

        public bool TryGetMoveSelector(UnitIndex unitIndex, out IMoveSelector moveSelector)
        {
            moveSelector = default;

            /*  If we don't have this index, stop!    */
            if (!this.unitIndexMoveSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            moveSelector = this.unitIndexMoveSelectionDictionary[unitIndex.Index];

            return true;
        }
    }
}