public class SceneData_UnitTurn
{
    public StationIndex SourceStationIndex { get; }
    public System.Collections.Generic.List<StationIndex> AllyStationIndexes { get; }
    public System.Collections.Generic.List<StationIndex> EnemyStationIndexes { get; }

    public SceneData_UnitTurn(StationIndex sourceUnitIndex, System.Collections.Generic.List<StationIndex> allyStationIndexes, System.Collections.Generic.List<StationIndex> enemyStationIndexes)
    {
        this.SourceStationIndex = sourceUnitIndex;
        this.AllyStationIndexes = allyStationIndexes;
        this.EnemyStationIndexes = enemyStationIndexes;
    }
}

public class UnitData_SceneData_UnitTurn
{
    public UnitData SourceUnitData { get; }
    public System.Collections.Generic.List<UnitData> AllyUnitData { get; }
    public System.Collections.Generic.List<UnitData> EnemyUnitData { get; }

    public UnitData_SceneData_UnitTurn(UnitData sourceUnitIndex, System.Collections.Generic.List<UnitData> allyStationIndexes, System.Collections.Generic.List<UnitData> enemyStationIndexes)
    {
        this.SourceUnitData = sourceUnitIndex;
        this.AllyUnitData = allyStationIndexes;
        this.EnemyUnitData = enemyStationIndexes;
    }
}