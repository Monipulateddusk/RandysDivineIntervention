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