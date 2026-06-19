using System.Linq;

namespace TurnBased.TurnOrder
{
    public class TurnOrderManager
    {
        private System.Collections.Generic.List<UnitIndex> UnitIndexTurnOrderList = new();
        private UnitIndex? currentUnit = null;

        public static event System.Action<System.Collections.Generic.List<UnitIndex>> OnUpdateTurnOrder;

        public void Awake()
        {
            this.UnitIndexTurnOrderList = new();

            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;
        }

        public void OnDestroy()
        {
            OnUpdateTurnOrder = null;
            StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;
        }

        private void StationManager_OnRemoveUnit(UnitIndex unitIndexOfTheRemovedUnit, StationIndex? theStationUnit, BaseBattleUnit battleUnitOfTheRemovedUnit)
        {
            /*  If the removed unit exists in our turn order, remove it.    */
            if (this.UnitIndexTurnOrderList.Contains(unitIndexOfTheRemovedUnit))
            {
                UnityEngine.Debug.LogError($"Removed Unit Index {unitIndexOfTheRemovedUnit.Index} from turnOrderList");
                this.UnitIndexTurnOrderList.Remove(unitIndexOfTheRemovedUnit);
            }
        }

        /// <summary>
        /// Called at the start of the round. If the current turn order is empty, then we want to make a new one and return true. If it was not empty, don't do anything and return false.
        /// </summary>
        /// <returns></returns>
        public TurnOrderCreationState TryCreateNewTurnOrderList(out System.Collections.Generic.List<UnitIndex> turnOrderList)
        {
            /*  If we didn't need to create a new turn order list.  */
            if(this.UnitIndexTurnOrderList.Count > 0)
            {
                turnOrderList = this.UnitIndexTurnOrderList;
                return TurnOrderCreationState.OldTurnOrderList;
            }
            else
            {
                turnOrderList = CreateTurnOrderList();

                /*  If we created a populated turn order list.  */
                if (turnOrderList.Count > 0)
                {
                    return TurnOrderCreationState.NewTurnOrderList;
                }
                /*  If we created a turn order list which is empty due to insufficient units.   */
                else
                {
                    return TurnOrderCreationState.InsufficentUnits;
                }
            }
        }

        private System.Collections.Generic.List<UnitIndex> CreateTurnOrderList()
        {
            this.currentUnit = null;    
            this.UnitIndexTurnOrderList = StationManager.Instance.GetAllActiveUnits();

            /*  Sort the List so that slowest Units are processed last. */
            SortTurnOrderList();

            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);

            return this.UnitIndexTurnOrderList;
        }

        private void SortTurnOrderList()
        {
            this.UnitIndexTurnOrderList.Sort((g1, g2) =>
            {
                Information.UnitInformationManager.Instance.TryGetCurrentSpeedOfUnitIndex(g1, out int unit1Speed);
                Information.UnitInformationManager.Instance.TryGetCurrentSpeedOfUnitIndex(g2, out int unit2Speed);

                return unit1Speed.CompareTo(unit2Speed);
            });

            this.UnitIndexTurnOrderList.Reverse();
        }

        public UnitIndex? PopNextUnitInTurnOrder()
        {
            if (!(this.UnitIndexTurnOrderList.Count > 0)) { return null; }

            this.currentUnit = UnitIndexTurnOrderList.FirstOrDefault();
            this.UnitIndexTurnOrderList.RemoveAt(0);
            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
            return currentUnit;
        }
        public void ResetCurrentUnit()
        {
            this.currentUnit = null;
            OnUpdateTurnOrder?.Invoke(this.UnitIndexTurnOrderList);
        }

        public System.Collections.Generic.List<UnitIndex> GetTurnOrderList() => this.UnitIndexTurnOrderList;
        public UnitIndex? GetCurrentUnit() => this.currentUnit;
    }
}