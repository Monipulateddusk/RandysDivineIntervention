using System.Collections.Generic;
using System.Linq;

namespace TurnBased.TargetSelection
{
    public interface ITargetSelector
    {
        public abstract System.Collections.Generic.List<StationIndex> SelectTargets(UnitTurnStationIndexesSceneData data, MoveTarget moveTargetType);
    }

    public class SequentialTargetSelector : ITargetSelector
    {
        // Yes, small bug is that if the unit selects to target their allies, it will increment their selection index, and then if they switch to enemies, it will carry over their selection index.
        int curSelectionIndex = 0;

        System.Collections.Generic.List<StationIndex> ITargetSelector.SelectTargets(UnitTurnStationIndexesSceneData data, MoveTarget moveTargetType)
        {
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);
            StationIndex selectedStationIndex;
            int wrappedIndex;



            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    WrapSelectionIndex(data.EnemyStationIndexes, curSelectionIndex, out wrappedIndex);

                    /*  After we get the selected station index, increment it for next time.    */
                    selectedStationIndex = data.EnemyStationIndexes[wrappedIndex];
                    curSelectionIndex++;

                    return new() { selectedStationIndex };
                }
                /// Single Ally
                else
                {
                    WrapSelectionIndex(data.AllyStationIndexes, curSelectionIndex, out wrappedIndex);

                    /*  After we get the selected station index, increment it for next time.    */
                    selectedStationIndex = data.AllyStationIndexes[wrappedIndex];
                    curSelectionIndex++;

                    return new() { selectedStationIndex };
                }
            }
        }

        private void WrapSelectionIndex(System.Collections.Generic.List<StationIndex> targettingStations, int selectionIndex, out int wrappedIndex)
        {
            wrappedIndex = selectionIndex;
            /*  Check to see if the index is valid, if not, wrap back to the start. */
            if (selectionIndex > targettingStations.Count - 1)
            {
                wrappedIndex = 0;
            }
        }    
    }

    public class RandomTargetSelector : ITargetSelector
    {
        public System.Collections.Generic.List<StationIndex> SelectTargets(UnitTurnStationIndexesSceneData data, MoveTarget moveTargetType)
        { 
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);


            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    return new() { GetRandomStationIndexFromList(data.EnemyStationIndexes) };
                }
                /// Single Ally
                else
                {

                    return new() { GetRandomStationIndexFromList(data.AllyStationIndexes) };
                }
            }
        }

        private StationIndex GetRandomStationIndexFromList(System.Collections.Generic.List<StationIndex> stationIndexes)
        {
            int randomIndex = UnityEngine.Random.Range(0, stationIndexes.Count);
            return stationIndexes[randomIndex];
        }
    }

    public class HighestHPTargetSelector : ITargetSelector
    {
        public List<StationIndex> SelectTargets(UnitTurnStationIndexesSceneData data, MoveTarget moveTargetType)
        {
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);


            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    return new() { GetHighestHPTargetFromList(data.EnemyStationIndexes) };
                }
                /// Single Ally
                else
                {
                    return new() { GetHighestHPTargetFromList(data.AllyStationIndexes) };
                }
            }
        }

        private StationIndex GetHighestHPTargetFromList(System.Collections.Generic.List<StationIndex> stationIndexes)
        {
            StationIndex highestHPUnit = stationIndexes.FirstOrDefault();
            int highestHPValue = 0;

            foreach (StationIndex stationIndex in stationIndexes)
            {
                if (!StationManager.Instance.TryGetUnitIndexOnStation(stationIndex, out UnitIndex  unitIndex)) { continue; }

                if (!Health.UnitHealthManager.Instance.TryGetCurrentHealthOfUnitIndex(unitIndex, out int unitHealth)) {  continue; }

                if (unitHealth > highestHPValue) 
                {  
                    highestHPValue = unitHealth; 
                    highestHPUnit = stationIndex;
                }
            }

            return highestHPUnit;
        }
    }


    public class PlayerDrivenTargetSelector : ITargetSelector
    {
        /// <summary>
        /// Can contain multiple targets or just one. By peeking at the selected move, we will know what to do with this List. I.e. Get the first index, or all targets. 
        /// </summary>
        public System.Collections.Generic.List<StationIndex> SelectedTarget { private get; set; }
        public System.Collections.Generic.List<StationIndex> SelectTargets(UnitTurnStationIndexesSceneData data, MoveTarget moveTargetType)
        {
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);


            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    /*  Confirm that the selected target is possible, if not pick the 0 index from the Enemy List.  */
                    if (DoesSelectedTargetExistInList(data.EnemyStationIndexes))
                    {
                        return this.SelectedTarget;
                    }

                    return new() { data.EnemyStationIndexes.FirstOrDefault()};
                }
                /// Single Ally
                else
                {
                    /*  Confirm that the selected target is possible, if not pick the 0 index from the Enemy List.  */
                    if (DoesSelectedTargetExistInList(data.AllyStationIndexes))
                    {
                        return this.SelectedTarget;
                    }

                    return new() { data.AllyStationIndexes.FirstOrDefault() };
                }
            }
        }

        private bool DoesSelectedTargetExistInList(System.Collections.Generic.List<StationIndex> List)
        {
            foreach (StationIndex stationIndex in List)
            {
                if(stationIndex.Index == SelectedTarget.FirstOrDefault().Index)
                {
                    return true;
                }
            }
            return false;   
        }
    }

}