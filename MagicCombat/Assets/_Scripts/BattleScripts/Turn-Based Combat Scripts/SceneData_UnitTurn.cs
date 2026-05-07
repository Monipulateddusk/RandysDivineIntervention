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



public class UnitDataUnitTurnSceneData
{
    public UnitData SourceUnitData                                              { get; }
    public System.Collections.Generic.List<UnitData> AllyUnitData               { get; }
    public System.Collections.Generic.List<UnitData> EnemyUnitData              { get; }

    public UnitDataUnitTurnSceneData(UnitData sourceUnitIndex, System.Collections.Generic.List<UnitData> allyStationIndexes, System.Collections.Generic.List<UnitData> enemyStationIndexes)
    {
        this.SourceUnitData = sourceUnitIndex;
        this.AllyUnitData = allyStationIndexes;
        this.EnemyUnitData = enemyStationIndexes;
    }
}