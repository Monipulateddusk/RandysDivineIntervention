
/// <summary>
/// This class acts as a container for information about a unit, it is a scriptable object so we can store those as files and read that info when creating a battlescene
/// </summary>
[UnityEngine.CreateAssetMenu(fileName = "UnitData", menuName = "UnitData", order = 1)]
public class UnitData : UnityEngine.ScriptableObject
{
    public new string name;

    public int maxHP;
    public int attack;
    public int speed;

    public UnityEngine.Color color;
    public UnityEngine.Sprite sprite;

    public Element element;
    
    [UnityEngine.SerializeReference, SubclassSelector]
    public System.Collections.Generic.List<TurnBased.IBattleMove> moves = new();

    public UnitMoveSelectorType moveSelectorType;
    public UnitTargetSelectorType targetSelectorType;

    public UnityEditor.Animations.AnimatorController unitAnimator;
}
