using System.Collections.Generic;

public class SceneData_UnitTurn
{
    public UnitIndex SourceUnitIndex { get; }
    public List<StationIndex?> AllyStationIndexes { get; }
    public List<StationIndex?> EnemyStationIndexes { get; }

    public SceneData_UnitTurn(UnitIndex sourceUnitIndex, List<StationIndex?> allyStationIndexes, List<StationIndex?> enemyStationIndexes)
    {
        this.SourceUnitIndex = sourceUnitIndex;
        this.AllyStationIndexes = allyStationIndexes;
        this.EnemyStationIndexes = enemyStationIndexes;
    }
}