
[UnityEngine.CreateAssetMenu(fileName = "LevelData", menuName = "LevelData", order = 1)]
public class LevelData : UnityEngine.ScriptableObject
{
    public string levelName;

    public System.Collections.Generic.List<UnitData> AllyUnitsInLevel = new();
    public System.Collections.Generic.List<UnitData> EnemyUnitsInLevel = new();
}