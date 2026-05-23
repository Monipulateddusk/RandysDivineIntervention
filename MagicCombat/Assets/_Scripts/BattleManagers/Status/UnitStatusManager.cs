using TurnBased.AttackResolution;

namespace TurnBased.Status
{
    public class UnitStatus
    {
        public System.Collections.Generic.List<BaseStatus> UnitStatuses { get; private set; }

        public UnitStatus()
        {
            this.UnitStatuses = new();
        }

        public bool AddStatusToList(BaseStatus statusToBeAdded)
        {
            if (statusToBeAdded == null || this.UnitStatuses == null) { return false; }

            /*  Check to see if the status to be added already exists and can be stacked. If not, we just add a new entry of the status to the list.    */
            foreach (BaseStatus status in this.UnitStatuses)
            {
                /*  If this status variety already exists in the List and is stackable, increment the stack size.   */
                if (status.GetType() == statusToBeAdded.GetType() && status.CanStack)
                {
                    status.IncrementStack();
                    return true;
                }
            }

            /*  If it doesn't exist in the list, or isn't stackable, add the incoming status to the list.   */
            this.UnitStatuses.Add(statusToBeAdded);
            return true;
        }

        public BaseStatus RemoveStatusFromList(BaseStatus statusToBeRemoved)
        {
            if (statusToBeRemoved == null || this.UnitStatuses == null) { return null; }
            
            for (int i = 0; i < this.UnitStatuses.Count; i++)
            {
                if (this.UnitStatuses[i] == statusToBeRemoved)
                {
                    if (this.UnitStatuses[i].DecrementStack())
                    {
                        this.UnitStatuses.Remove(statusToBeRemoved);
                        return statusToBeRemoved;
                    }
                }
            }
            return null;
        }        
    }

    public class UnitStatusManager
    {
        private static UnitStatusManager instance;
        public static UnitStatusManager Instance
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

        private System.Collections.Generic.Dictionary<int, UnitStatus> UnitStatusDictionary = new();


        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.UnitStatusDictionary = new();

            StationManager.OnAddUnit += AddUnitIndexToDictionary;
            StationManager.OnRemoveUnit += RemoveUnitIndexFromDictionary;
        }



        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.UnitStatusDictionary.Clear();

            StationManager.OnAddUnit -= AddUnitIndexToDictionary;
            StationManager.OnRemoveUnit -= RemoveUnitIndexFromDictionary;
        }

        public void AddUnitIndexToDictionary(UnitIndex unitIndex)
        {
            if (this.UnitStatusDictionary.ContainsKey(unitIndex.Index)) { return; }

            UnitStatusDictionary.Add(unitIndex.Index, new());
        }

        private void RemoveUnitIndexFromDictionary(UnitIndex unitIndex, StationIndex? arg2, BaseBattleUnit arg3)
        {
            RemoveStatusFromDictionary(unitIndex);
        }

        public void RemoveStatusFromDictionary(UnitIndex unitIndex)
        {
            if (!this.UnitStatusDictionary.ContainsKey(unitIndex.Index)) { return; }

            UnitStatusDictionary.Remove(unitIndex.Index);
        }

        public bool TryGetStatusOfUnitIndex(UnitIndex unitIndex, out UnitStatus unitStatus)
        {
            unitStatus = default;
            if (!this.UnitStatusDictionary.ContainsKey(unitIndex.Index)) { return false; }

            unitStatus = this.UnitStatusDictionary[unitIndex.Index];
            return true;    
        }
    }


    public static class UnitStatusHandler
    {
        public static void AddStatusForUnitIndex(UnitIndex unitIndex, Status.BaseStatus statusBeingAdded)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            unitStatus.AddStatusToList(statusBeingAdded);
        }
        public static void RemoveStatusForUnitIndex(UnitIndex unitIndex, Status.BaseStatus statusBeingRemoved)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            
        }

        public static void ModifyIncomingDamageRequestToUnitIndex(UnitIndex unitIndex, AttackResolution.DamageRequest damageRequest)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            foreach (BaseStatus status in unitStatus.UnitStatuses)
            {
                status.ModifyIncomingDamageRequest(damageRequest);
            }
        }

        public static void ModifyOutgoingDamageRequestFromUnitIndex(UnitIndex unitIndex, AttackResolution.DamageRequest damageRequest)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            foreach (BaseStatus status in unitStatus.UnitStatuses)
            {
                status.ModifyOutgoingDamageRequest(damageRequest);
            }
        }

        public static void ModifyIncomingHealRequestToUnitIndex(UnitIndex unitIndex, AttackResolution.HealRequest healRequest)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            foreach (BaseStatus status in unitStatus.UnitStatuses)
            {
                status.ModifyIncomingHealRequest(healRequest);
            }
        }

        public static void ModifyOutgoingHealRequestFromUnitIndex(UnitIndex unitIndex, AttackResolution.HealRequest healRequest)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            foreach (BaseStatus status in unitStatus.UnitStatuses)
            {
                status.ModifyOutgoingHealRequest(healRequest);
            }
        }

        public static void ModifyIncomingApplyStatusRequestToUnitIndex(UnitIndex unitIndex, AttackResolution.ApplyStatusRequest statusRequest)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            foreach (BaseStatus status in unitStatus.UnitStatuses)
            {
                status.ModifyIncomingApplyStatusRequest(statusRequest);
            }
        }

        public static void ModifyOutgoingApplyStatusRequestFromUnitIndex(UnitIndex unitIndex, AttackResolution.ApplyStatusRequest statusRequest)
        {
            if (!UnitStatusManager.Instance.TryGetStatusOfUnitIndex(unitIndex, out UnitStatus unitStatus)) { return; }

            foreach (BaseStatus status in unitStatus.UnitStatuses)
            {
                status.ModifyOutgoingApplyStatusRequest(statusRequest);
            }
        }
    }
}