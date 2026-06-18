namespace TurnBased.Information {

    public class UnitInformation
    {
        public int UnitCurrentHealth { get; private set; }
        public int UnitMaxHealth { get; private set; }
        public int UnitAttack { get; private set; }
        public int UnitSpeed { get; private set; }
        public UnitTeam Team { get; private set; }
        public Element UnitElement { get; private set; }
        public UnitIndex UnitIndex { get; private set; }

        public UnitInformation(int unitCurrentHealth, int unitMaxHealth, int unitAttack, int unitSpeed, UnitTeam team, Element unitElement, UnitIndex unitIndex)
        {
            this.UnitCurrentHealth = unitCurrentHealth;
            this.UnitMaxHealth = unitMaxHealth;
            this.UnitAttack = unitAttack;
            this.UnitSpeed = unitSpeed;
            this.Team = team;
            this.UnitElement = unitElement;
            this.UnitIndex = unitIndex;
        }

        /// <summary>
        /// Heals based on the Damage Amount keeping within Max Health and 0.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <returns>New Current Health</returns>
        public int HealWithHealValue(int healAmount)
        {
            /*  Get the new health value taking our current health adding on the heal amount    */
            int newHealthValue = this.UnitCurrentHealth + healAmount;

            /*  Don't heal past the maximum health value.   */
            newHealthValue = UnityEngine.Mathf.Clamp(newHealthValue, 0, this.UnitMaxHealth);

            this.UnitCurrentHealth = newHealthValue;
            return this.UnitCurrentHealth;
        }

        /// <summary>
        /// Damages based on the Damage Amount keeping within Max Health and 0.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <returns>New Current Health</returns>
        public int DamageWithDamageValue(int damageAmount)
        {
            /*  Subtract the incoming damage with the health we currently have. */
            int newHealthValue = this.UnitCurrentHealth - damageAmount;

            /*  Don't go below 0.   */
            newHealthValue = UnityEngine.Mathf.Clamp(newHealthValue, 0, this.UnitMaxHealth);

            this.UnitCurrentHealth = newHealthValue;
            return this.UnitCurrentHealth;
        }

        public bool SetHealthValue(int newHealthValue)
        {
            /*  Don't set the health value to something higher than our maximum health. */
            if (newHealthValue > this.UnitMaxHealth) { return false; }

            this.UnitCurrentHealth = newHealthValue;
            return true;
        }

        public bool IsHealthZero() => this.UnitCurrentHealth <= 0;

        public int IncreaseSpeedByValue(int speedIncreaseValue)
        {
            this.UnitSpeed += speedIncreaseValue;

            return this.UnitSpeed;
        }

        public int DecreaseSpeedByValue(int speedDecreaseValue)
        {
            this.UnitSpeed -= speedDecreaseValue;

            return this.UnitSpeed;
        }
    }

    public class UnitInformationManager
    {
        private static UnitInformationManager instance;
        public static UnitInformationManager Instance
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

        public static event System.Action<UnitIndex, UnitInformation>   OnUnitInformationAdded;
        public static event System.Action<UnitIndex, int>               OnUnitHealthChange;
        public static event System.Action<UnitIndex, int>               OnUnitSpeedChange;
        public static event System.Action<UnitIndex>                    OnUnitInformationRemoved;

        private System.Collections.Generic.Dictionary<int, UnitInformation> UnitIndexInformationDict = new();

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.UnitIndexInformationDict = new();
            
            StationManager.OnAddUnit    += StationManager_OnAddUnit;
            StationManager.OnRemoveUnit += StationManager_OnRemoveUnit;
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.UnitIndexInformationDict.Clear();

            StationManager.OnAddUnit    -= StationManager_OnAddUnit;
            StationManager.OnRemoveUnit -= StationManager_OnRemoveUnit;

            OnUnitInformationAdded = null;
            OnUnitHealthChange = null;
            OnUnitSpeedChange = null;
            OnUnitInformationRemoved = null;
        }

        private void StationManager_OnAddUnit(UnitIndex unitIndex)
        {
            if(!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndex, out UnitData unitData)) { return; }
            if(!StationManager.Instance.TryGetTeamOfUnitIndex(unitIndex, out UnitTeam team)) { return; }

            AddUnitInformationToDictionary(unitIndex, unitData, team);
        }

        private void StationManager_OnRemoveUnit(UnitIndex unitIndexOfRemovedUnit, StationIndex? stationOfUnitIndex, BaseBattleUnit battleUnitOfRemovedUnit)
        {
            UnityEngine.Debug.LogError("Starting to remove unit index from UnitHealthManager");

            RemoveUnitInformationFromDictionary(unitIndexOfRemovedUnit);

            UnityEngine.Debug.LogError("Removed unit index from UnitHealthManager");
        }

        public bool AddUnitInformationToDictionary(UnitIndex unitIndex, UnitData data, UnitTeam team)
        {
            if (this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            UnitInformation addedUnitInformation = new(data.maxHP, data.maxHP, data.attack, data.speed, team, data.element, unitIndex);

            this.UnitIndexInformationDict.Add(unitIndex.Index, addedUnitInformation);
            OnUnitInformationAdded?.Invoke(unitIndex, addedUnitInformation);
            return true;
        }

        public bool RemoveUnitInformationFromDictionary(UnitIndex unitIndex)
        {
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            this.UnitIndexInformationDict.Remove(unitIndex.Index);
            OnUnitInformationRemoved?.Invoke(unitIndex);
            return true;
        }

        public bool SetUnitHealth(UnitIndex unitIndex, int newHealthValue)
        {
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            if (!this.UnitIndexInformationDict[unitIndex.Index].SetHealthValue(newHealthValue)) { return false; }

            OnUnitHealthChange?.Invoke(unitIndex, newHealthValue);
            return true;
        }

        public bool HealUnitByHealAmount(UnitIndex unitIndex, int healAmount)
        {
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            int newHealthValue = this.UnitIndexInformationDict[unitIndex.Index].HealWithHealValue(healAmount);

            OnUnitHealthChange?.Invoke(unitIndex, newHealthValue);
            return true;
        }

        public bool DamageUnitByDamageAmount(UnitIndex unitIndex, int damageAmount)
        {
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            int newHealthValue = this.UnitIndexInformationDict[unitIndex.Index].DamageWithDamageValue(damageAmount);

            OnUnitHealthChange?.Invoke(unitIndex, newHealthValue);
            return true;
        }

        public bool IncreaseSpeedOfUnitByAmount(UnitIndex unitIndex, int amount)
        {
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            int newSpeedValue = this.UnitIndexInformationDict[unitIndex.Index].IncreaseSpeedByValue(amount);
            OnUnitSpeedChange?.Invoke(unitIndex, newSpeedValue);
            return true;
        }

        public bool DecreaseSpeedOfUnitByAmount(UnitIndex unitIndex, int amount)
        {
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            int newSpeedValue = this.UnitIndexInformationDict[unitIndex.Index].DecreaseSpeedByValue(amount);
            OnUnitSpeedChange?.Invoke(unitIndex, newSpeedValue);
            return true;
        }

        public bool TryGetCurrentHealthOfUnitIndex(UnitIndex unitIndex, out int currentHealth)
        {
            currentHealth = default;
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            currentHealth = this.UnitIndexInformationDict[unitIndex.Index].UnitCurrentHealth;
            return true;
        }

        public bool TryGetMaximumHealthOfUnitIndex(UnitIndex unitIndex, out int maximumHealth)
        {
            maximumHealth = default;
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            maximumHealth = this.UnitIndexInformationDict[unitIndex.Index].UnitMaxHealth;
            return true;
        }

        public bool TryGetCurrentSpeedOfUnitIndex(UnitIndex unitIndex, out int currentSpeed)
        {
            currentSpeed = default;
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            currentSpeed = this.UnitIndexInformationDict[unitIndex.Index].UnitSpeed;
            return true;
        }

        public bool TryGetUnitInformationOfUnitIndex(UnitIndex unitIndex, out UnitInformation unitInformation)
        {
            unitInformation = default;
            if (!this.UnitIndexInformationDict.ContainsKey(unitIndex.Index)) { return false; }

            unitInformation = this.UnitIndexInformationDict[unitIndex.Index];
            return true;
        }

        /// <summary>
        /// Called to check through all Units. If any of them have health values that are 0, create a list of all of those Units. Don't yet remove them we may want to do things with them. 
        /// </summary>
        /// <returns></returns>
        internal System.Collections.Generic.List<UnitIndex> GetAllUnitsWithNoHealth()
        {
            System.Collections.Generic.List<UnitIndex> noHealthUnits = new();

            foreach(System.Collections.Generic.KeyValuePair<int, UnitInformation> unitHealthKeyValuePair in this.UnitIndexInformationDict)
            {
                int unitIndexInt = unitHealthKeyValuePair.Key;
                UnitInformation unitInformation = unitHealthKeyValuePair.Value;
                
                if (unitInformation.IsHealthZero()) { noHealthUnits.Add(new UnitIndex(unitIndexInt)); }

                continue;
            }

            return noHealthUnits;
        }
    }
}