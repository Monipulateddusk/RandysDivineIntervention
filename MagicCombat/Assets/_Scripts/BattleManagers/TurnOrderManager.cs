using System.Linq;

namespace TurnBased.TurnOrder
{
    public class TurnOrderManager
    {
        private static TurnOrderManager instance;
        public static TurnOrderManager Instance
        {
            get
            {
                try
                {
                    return instance;
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                    return null;
                }
            }
        }

        private System.Collections.Generic.List<UnitIndex> UnitIndexTurnOrderList = new();
        private UnitIndex currentUnit;

        public static event System.Action<System.Collections.Generic.List<UnitIndex>> OnUpdateTurnOrder;

        public void Awake()
        {
            instance = this;
        }

        public System.Collections.Generic.List<UnitIndex> CreateTurnOrderList()
        {
            this.UnitIndexTurnOrderList = StationManager.Instance.GetAllActiveUnits();

            /*  Sort the List so that slowest Units are processed last. */
            this.UnitIndexTurnOrderList.Sort((g1, g2) =>
            {
                StationManager.Instance.TryGetBattleUnitOfIndex(g1, out BaseBattleUnit unit1);
                StationManager.Instance.TryGetBattleUnitOfIndex(g2, out BaseBattleUnit unit2);

                return unit1.GetBaseUnit().speed.CompareTo(unit2.GetBaseUnit().speed);
            });

            this.UnitIndexTurnOrderList.Reverse();

            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
            return this.UnitIndexTurnOrderList;
        }

        public UnitIndex? PopNextUnitInTurnOrder()
        {
            if (!(this.UnitIndexTurnOrderList.Count > 0)) { return null; }

            currentUnit = UnitIndexTurnOrderList.FirstOrDefault();
            UnitIndexTurnOrderList.RemoveAt(0);
            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
            return currentUnit;

        }

        public System.Collections.Generic.List<UnitIndex> GetTurnOrderList() => this.UnitIndexTurnOrderList;
        public UnitIndex GetCurrentUnit() => this.currentUnit;
    }
}