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
                UnityEngine.Debug.LogError($"Removing unit of UnitIndex {unitIndex.Index}");

                StationManager.Instance.RemoveUnit(unitIndex);
            }


            /*  Finally, determine the Game State. */
            TurnBased.GameState.GameStateManager.Instance.DetermineGameState();
        }

        public static System.Collections.Generic.List<UnitIndex> GetAllDeadUnits()
        {
            /*  For each unit on both teams, if any of them have a health value of 0, Tell the StationManager to remove them.  */
            System.Collections.Generic.List<UnitIndex> deadUnits = new();
            
            foreach (UnitIndex unitIndex in StationManager.Instance.GetUnitsOnTeam(UnitTeam.ALLY))
            {
                if (!Health.UnitHealthManager.Instance.TryGetCurrentHealthOfUnitIndex(unitIndex, out int currentHealth)) { continue; }

                if (currentHealth <= 0)
                {
                    deadUnits.Add(unitIndex);
                    continue;
                }
            }

            foreach (UnitIndex unitIndex in StationManager.Instance.GetUnitsOnTeam(UnitTeam.ENEMY))
            {
                if (!Health.UnitHealthManager.Instance.TryGetCurrentHealthOfUnitIndex(unitIndex, out int currentHealth)) { continue; }

                if (currentHealth <= 0)
                {
                    deadUnits.Add(unitIndex);
                    continue;
                }
            }

            return deadUnits;
        }
    }
}