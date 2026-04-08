using System.Collections.Generic;
using TurnBased;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// This class acts as a container for information about a unit, it is a scriptable object so we can store those as files and read that info when creating a battlescene
/// </summary>
[CreateAssetMenu(fileName = "UnitData", menuName = "UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    public new string name;

    public int maxHP;
    public int attack;
    public int speed;

    public Color color;
    public Sprite sprite;

    public Element element;
    
    [SerializeReference, SubclassSelector]
    public List<IBattleMove> moves = new();

    public UnitMoveSelectorType moveSelectorType;

    public AnimatorController unitAnimator;
}
