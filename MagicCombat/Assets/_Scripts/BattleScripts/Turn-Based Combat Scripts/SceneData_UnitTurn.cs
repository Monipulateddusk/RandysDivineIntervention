public class SceneDataStationIndexes
{
    public System.Collections.Generic.List<StationIndex> AllyStationIndexes { get; }
    public System.Collections.Generic.List<StationIndex> EnemyStationIndexes { get; }

    public SceneDataStationIndexes(System.Collections.Generic.List<StationIndex> allyStationIndexes, System.Collections.Generic.List<StationIndex> enemyStationIndexes)
    {
        this.AllyStationIndexes = allyStationIndexes;
        this.EnemyStationIndexes = enemyStationIndexes;
    }
}

public class UnitTurnStationIndexesSceneData : SceneDataStationIndexes
{
    public StationIndex SourceStationIndex                                      { get; }

    public UnitTurnStationIndexesSceneData(StationIndex sourceUnitIndex, 
        System.Collections.Generic.List<StationIndex> allyStationIndexes, 
        System.Collections.Generic.List<StationIndex> enemyStationIndexes) : base (allyStationIndexes, enemyStationIndexes)
    {
        this.SourceStationIndex = sourceUnitIndex;
    }
}