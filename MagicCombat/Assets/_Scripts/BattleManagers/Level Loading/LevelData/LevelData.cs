
[UnityEngine.CreateAssetMenu(fileName = "LevelData", menuName = "LevelData", order = 1)]
public class LevelData : UnityEngine.ScriptableObject
{
    public string levelName;

    [UnityEngine.SerializeField] public System.Collections.Generic.List<UnitData> AllyUnitsInLevel = new();
    [UnityEngine.SerializeField] public System.Collections.Generic.List<UnitData> EnemyUnitsInLevel = new();
}