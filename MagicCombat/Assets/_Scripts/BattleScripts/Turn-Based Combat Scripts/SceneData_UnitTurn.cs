using System.Collections.Generic;

public class SceneData_UnitTurn
{
    public UnitIndex SourceUnitIndex;
    public List<StationIndex?> AllyStationIndexes;
    public List<StationIndex?> EnemyStationIndexes;

    public SceneData_UnitTurn(UnitIndex sourceUnitIndex, List<StationIndex?> allyStationIndexes, List<StationIndex?> enemyStationIndexes)
    {
        this.SourceUnitIndex = sourceUnitIndex;
        this.AllyStationIndexes = allyStationIndexes;
        this.EnemyStationIndexes = enemyStationIndexes;
    }
}