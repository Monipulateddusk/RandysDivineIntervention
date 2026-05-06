using System.Linq;
using UnityEngine;

namespace TurnBased.TargetSelection
{
    public class UnitTargetSelectorManager
    {
        private static UnitTargetSelectorManager instance;
        public static UnitTargetSelectorManager Instance
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

        private System.Collections.Generic.Dictionary<int, ITargetSelector> UnitIndexTargetSelectionDictionary = new();

        public void Awake()
        {
            /*  Initalise the Singleton.    */
            if (instance == null)
            {
                instance = this;
            }

            this.UnitIndexTargetSelectionDictionary = new();
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
            System.Collections.Generic.List<int> keys = this.UnitIndexTargetSelectionDictionary.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                this.UnitIndexTargetSelectionDictionary.Remove(keys[i]);
            }
            this.UnitIndexTargetSelectionDictionary.Clear();
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
            if (this.UnitIndexTargetSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            /*  When called, retrieve the IMoveSelector type.   */
            if (!TargetSelectorHandler.TryGetSelector(targetSelectorType, out ITargetSelector targetSelector)) { return false; }


            if (TargetSelectorHandler.IsSelectorTypeInstanciatable(targetSelectorType))
            {
                /* Create a new instance of the class. Important for selectors like Sequential which have a unit-driven 'memory' for the previously selected move.  */
                ITargetSelector newSelectorInstance = (ITargetSelector)System.Activator.CreateInstance(targetSelector.GetType());

                this.UnitIndexTargetSelectionDictionary.Add(unitIndex.Index, newSelectorInstance);
            }
            /*  If not, just get a referance to this Class. */
            else
            {
                this.UnitIndexTargetSelectionDictionary.Add(unitIndex.Index, targetSelector);
            }

            return true;
        }

        public bool RemoveUnitTargetSelector(UnitIndex unitIndex)
        {
            /*  If we don't have this index, stop!    */
            if (!this.UnitIndexTargetSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexTargetSelectionDictionary.Remove(unitIndex.Index);
            return true;
        }

        public bool TryGetTargetSelector(UnitIndex unitIndex, out ITargetSelector targetSelector)
        {
            targetSelector = default;

            /*  If we don't have this index, stop!    */
            if (!this.UnitIndexTargetSelectionDictionary.ContainsKey(unitIndex.Index)) { return false; }



            targetSelector = this.UnitIndexTargetSelectionDictionary[unitIndex.Index];

            UnityEngine.Debug.Log(targetSelector);
            return true;
        }
    }
}