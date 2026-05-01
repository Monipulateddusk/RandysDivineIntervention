using System.Linq;
using TurnBased.MoveSelection;
using UnityEngine;

namespace TurnBased.TargetSelection
{
    public class TargetSelectorManager
    {
        private static TargetSelectorManager instance;
        public static TargetSelectorManager Instance
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

        private System.Collections.Generic.Dictionary<int, ITargetSelector> unitIndexTargetSelectionDictionary = new();

        public void Awake()
        {
            /*  Initalise the Singleton.    */
            if (instance == null)
            {
                instance = this;
            }

            this.unitIndexTargetSelectionDictionary = new();
            StationManager.OnAddUnit += StationManager_OnAddUnit;
            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            StationManager.OnAddUnit -= StationManager_OnAddUnit;
            StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;

            RemoveAllUnitIndexesFromDictionary();
        }


        private void RemoveAllUnitIndexesFromDictionary()
        {
            System.Collections.Generic.List<int> keys = this.unitIndexTargetSelectionDictionary.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                this.unitIndexTargetSelectionDictionary.Remove(keys[i]);
            }
            this.unitIndexTargetSelectionDictionary.Clear();
        }

        private void StationManager_OnAddUnit(UnitIndex unitIndex)
        {
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit unit)) { return; }
            AddUnitTargetSelector(unitIndex, unit.GetBaseUnit().targetSelectorType);
        }

        private void StationManager_OnRemoveUnit(UnitIndex unitIndex, StationIndex? stationIndex, BaseBattleUnit unit)
        {
            UnityEngine.Debug.LogError("Starting to remove unit from TargetSelectorManager");

            RemoveUnitTargetSelector(unitIndex);

            UnityEngine.Debug.LogError("Removed unit from TargetSelectorManager");
        }

        public bool AddUnitTargetSelector(UnitIndex unitIndex, UnitTargetSelectorType targetSelectorType)
        {
            /*  If we already have this index, stop!    */
            if (this.unitIndexTargetSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            /*  When called, retrieve the IMoveSelector type.   */
            if (!TargetSelectorHandler.TryGetSelector(targetSelectorType, out ITargetSelector targetSelector)) { return false; }


            if (TargetSelectorHandler.IsSelectorTypeInstanciatable(targetSelectorType))
            {
                /* Create a new instance of the class. Important for selectors like Sequential which have a unit-driven 'memory' for the previously selected move.  */
                ITargetSelector newSelectorInstance = (ITargetSelector)System.Activator.CreateInstance(targetSelector.GetType());

                this.unitIndexTargetSelectionDictionary.Add(unitIndex.Index, newSelectorInstance);
            }
            /*  If not, just get a referance to this Class. */
            else
            {
                this.unitIndexTargetSelectionDictionary.Add(unitIndex.Index, targetSelector);
            }

           // UnityEngine.Debug.Log("Creating target selector of type: " + targetSelector.ToString() + " for index: " + unitIndex.Index);

            return true;
        }

        public bool RemoveUnitTargetSelector(UnitIndex unitIndex)
        {
            /*  If we don't have this index, stop!    */
            if (!this.unitIndexTargetSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            this.unitIndexTargetSelectionDictionary.Remove(unitIndex.Index);
            return true;
        }

        public bool TryGetTargetSelector(UnitIndex unitIndex, out ITargetSelector targetSelector)
        {
            targetSelector = default;

            /*  If we don't have this index, stop!    */
            if (!this.unitIndexTargetSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }



            targetSelector = this.unitIndexTargetSelectionDictionary[unitIndex.Index];

            UnityEngine.Debug.Log(targetSelector);
            return true;
        }
    }
}