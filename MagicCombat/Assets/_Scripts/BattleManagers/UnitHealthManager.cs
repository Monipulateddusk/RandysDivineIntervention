namespace TurnBased.Health {

    public class UnitHealthData
    {
        private int curHealth;
        public int CurrentHealth { get { return curHealth; } 
            set 
            {
                curHealth = value; 
            } 
        }
        public int MaximumHealth { get; }

        public UnitHealthData(int maxHealthValue)
        {
            this.CurrentHealth = maxHealthValue; 
            this.MaximumHealth = maxHealthValue;
        }

        public bool SetHealthValue(int newHealthValue)
        {
            /*  Don't set the health value to something higher than our maximum health. */
            if(newHealthValue > this.MaximumHealth) { return false; }

            this.CurrentHealth = newHealthValue;    
            return true;
        }

        /// <summary>
        /// Heals based on the Damage Amount keeping within Max Health and 0.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <returns>New Current Health</returns>
        public int HealWithHealValue(int healAmount)
        {
            /*  Get the new health value taking our current health adding on the heal amount    */
            int newHealthValue = this.CurrentHealth + healAmount;

            /*  Don't heal past the maximum health value.   */
            newHealthValue = UnityEngine.Mathf.Clamp(newHealthValue, 0, this.MaximumHealth);

            this.CurrentHealth = newHealthValue;
            return this.CurrentHealth;
        }

        /// <summary>
        /// Damages based on the Damage Amount keeping within Max Health and 0.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <returns>New Current Health</returns>
        public int DamageWithDamageValue(int damageAmount)
        {
            /*  Subtract the incoming damage with the health we currently have. */
            int newHealthValue = this.CurrentHealth - damageAmount;

            /*  Don't go below 0.   */
            newHealthValue = UnityEngine.Mathf.Clamp(newHealthValue, 0, this.MaximumHealth);

            this.CurrentHealth = newHealthValue;
            return this.CurrentHealth;
        }

        public bool IsHealthZero() => this.CurrentHealth <= 0;
    }


    public class UnitHealthManager
    {
        private static UnitHealthManager instance;
        public static UnitHealthManager Instance
        {
            get
            {
                return instance;
            }

            set
            {
                if (instance == null)
                {
                    instance = value;
                }
            }
        }

        public static event System.Action<UnitIndex, int>   OnUnitHealthAdded;
        public static event System.Action<UnitIndex, int>   OnUnitHealthChange;
        public static event System.Action<UnitIndex>        OnUnitHealthRemoved;

        private readonly System.Collections.Generic.Dictionary<int, UnitHealthData> UnitIndexHealthDict = new();

        public void Awake()
        {
            Instance = this;

            StationManager.OnAddUnit    += StationManager_OnAddUnit;
            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;
        }

        public void OnDestroy()
        {
            StationManager.OnAddUnit    -= StationManager_OnAddUnit;
            StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;
        }

        private void StationManager_OnAddUnit(UnitIndex unitIndex)
        {
            if(!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndex, out UnitData unitData)) { return; }

            AddUnitHealthToDictionary(unitIndex, unitData.maxHP);
        }

        private void StationManager_OnRemoveUnit(UnitIndex unitIndexOfRemovedUnit, StationIndex? stationOfUnitIndex, BaseBattleUnit battleUnitOfRemovedUnit)
        {
            RemoveUnitHealthToDictionary(unitIndexOfRemovedUnit);
        }

        public bool AddUnitHealthToDictionary(UnitIndex unitIndex, int maximumHealth)
        {
            if (this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexHealthDict.Add(unitIndex.Index, new(maximumHealth));
            OnUnitHealthAdded?.Invoke(unitIndex, maximumHealth);
            return true;
        }

        public bool RemoveUnitHealthToDictionary(UnitIndex unitIndex)
        {
            if (!this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexHealthDict.Remove(unitIndex.Index);
            OnUnitHealthRemoved?.Invoke(unitIndex);
            return true;
        }

        public bool SetUnitHealth(UnitIndex unitIndex, int newHealthValue)
        {
            if (!this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            if (!this.UnitIndexHealthDict[unitIndex.Index].SetHealthValue(newHealthValue)) { return false; }

            OnUnitHealthChange?.Invoke(unitIndex, newHealthValue);
            return true;
        }

        public bool HealUnitByHealAmount(UnitIndex unitIndex, int healAmount)
        {
            if (!this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            /*  Get the current*/
            int newHealthValue = this.UnitIndexHealthDict[unitIndex.Index].HealWithHealValue(healAmount);

            OnUnitHealthChange?.Invoke(unitIndex, newHealthValue);
            return true;
        }

        public bool DamageUnitByDamageAmount(UnitIndex unitIndex, int damageAmount)
        {
            if (!this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            /*  Get the current*/
            int newHealthValue = this.UnitIndexHealthDict[unitIndex.Index].DamageWithDamageValue(damageAmount);

            OnUnitHealthChange?.Invoke(unitIndex, newHealthValue);
            return true;
        }

        public bool GetCurrentHealthOfUnitIndex(UnitIndex unitIndex, out int currentHealth)
        {
            currentHealth = default;
            if (!this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            currentHealth = this.UnitIndexHealthDict[unitIndex.Index].CurrentHealth;
            return true;
        }

        public bool GetMaximumHealthOfUnitIndex(UnitIndex unitIndex, out int maximumHealth)
        {
            maximumHealth = default;
            if (!this.UnitIndexHealthDict.ContainsKey(unitIndex.Index)) { return false; }

            maximumHealth = this.UnitIndexHealthDict[unitIndex.Index].MaximumHealth;
            return true;
        }


        /// <summary>
        /// Called to check through all Units. If any of them have health values that are 0, create a list of all of those Units. Don't yet remove them we may want to do things with them. 
        /// </summary>
        /// <returns></returns>
        internal System.Collections.Generic.List<UnitIndex> GetAllUnitsWithNoHealth()
        {
            System.Collections.Generic.List<UnitIndex> noHealthUnits = new();

            foreach(System.Collections.Generic.KeyValuePair<int, UnitHealthData> unitHealthKeyValuePair in this.UnitIndexHealthDict)
            {
                int unitIndexInt = unitHealthKeyValuePair.Key;
                UnitHealthData unitHealthData = unitHealthKeyValuePair.Value;
                
                if (unitHealthData.IsHealthZero()) { noHealthUnits.Add(new UnitIndex(unitIndexInt)); }

                continue;
            }

            return noHealthUnits;
        }
    }
}