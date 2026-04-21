using System.Linq;
using UnityEngine;

namespace TurnBased.Intention
{
    public static class IntentionResolverUtility
    {
        public static System.Collections.Generic.List<UnitIndex> GetAllAutonomousUnits()
        {
            System.Collections.Generic.List<UnitIndex> autonomousUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { continue; }

                if (moveSelector is not MoveSelection.PlayerDrivenMoveSelector)
                {
                    autonomousUnits.Add(unitIndex);
                }
            }
            return autonomousUnits;
        }

        public static System.Collections.Generic.List<UnitIndex> GetAllPlayerDrivenUnits()
        {
            System.Collections.Generic.List<UnitIndex> playerDrivenUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { continue; }

                if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
                {
                    playerDrivenUnits.Add(unitIndex);
                }
            }
            return playerDrivenUnits;
        }

        public static bool DoesListContainUnitIndex(UnitIndex unitIndex, System.Collections.Generic.List<UnitIndex> listOfUnitIndexes)
        {
            int index = unitIndex.Index;

            foreach (UnitIndex uIndex in listOfUnitIndexes)
            {
                if (uIndex.Index == index)
                {
                    return true;
                }
            }
            return false;
        }
    }
}