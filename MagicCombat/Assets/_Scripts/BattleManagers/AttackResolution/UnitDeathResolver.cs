namespace TurnBased.AttackResolution
{
    public static class UnitDeathResolver
    {
        /// <summary>
        ///  When called after an attack is fully resolved, we need to know if any Units died as a result of the attack. 
        /// </summary>
        public static void DetermineDeadUnits()
        { 
            /*  Get all units on both teams in resurve and on the field.    */
            System.Collections.Generic.List<UnitIndex> allDeadUnits = GetAllDeadUnits();

            /*  Remove all the dead units.  */
            foreach (UnitIndex unitIndex in allDeadUnits)
            {
                UnityEngine.Debug.LogError($"Removing unit of UnitIndex {unitIndex.Index}. Station manager is: {StationManager.Instance}");

                bool res = StationManager.Instance.RemoveUnit(unitIndex);

                UnityEngine.Debug.LogError($"Result of removing is: {res}");
            }
            UnityEngine.Debug.LogWarning($"Determining game state");

            /*  Finally, determine the Game State. */
            TurnBased.GameState.GameStateManager.Instance.DetermineGameState();
        }

        public static System.Collections.Generic.List<UnitIndex> GetAllDeadUnits()
        {
            /*  For each unit on both teams, if any of them have a health value of 0, Tell the StationManager to remove them.  */
            System.Collections.Generic.List<UnitIndex> deadUnits = new();

            UnityEngine.Debug.LogWarning($"Getting allied dead units");

            foreach (UnitIndex unitIndex in StationManager.Instance.GetUnitsOnTeam(UnitTeam.ALLY))
            {
                if (!Information.UnitInformationManager.Instance.TryGetCurrentHealthOfUnitIndex(unitIndex, out int currentHealth)) { continue; }

                if (currentHealth <= 0)
                {
                    deadUnits.Add(unitIndex);
                    continue;
                }
            }


            UnityEngine.Debug.LogWarning($"Getting enemy dead units");

            foreach (UnitIndex unitIndex in StationManager.Instance.GetUnitsOnTeam(UnitTeam.ENEMY))
            {
                if (!Information.UnitInformationManager.Instance.TryGetCurrentHealthOfUnitIndex(unitIndex, out int currentHealth)) { continue; }

                if (currentHealth <= 0)
                {
                    deadUnits.Add(unitIndex);
                    continue;
                }
            }


            UnityEngine.Debug.LogWarning($"Dead unit total is: {deadUnits.Count}");

            return deadUnits;
        }
    }
}