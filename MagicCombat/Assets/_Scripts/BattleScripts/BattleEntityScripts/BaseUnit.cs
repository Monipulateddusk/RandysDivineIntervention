using System.Collections.Generic;
using TurnBased;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Same order as 'EnvironmentalEffect' enum in Moves Class for easy integer conversion between the two
/// </summary>
public enum Element
{
    FIRE, WATER, ICE, EARTH, LIGHT, DARKNESS, NULL,
}
/// <summary>
/// This class acts as a container for information about a unit, it is a scriptable object so we can store those as files and read that info when creating a battlescene
/// </summary>
[CreateAssetMenu(fileName = "EnemyUnit", menuName = "ScriptableObjects/Unit", order = 1)]
public class BaseUnit : ScriptableObject
{
    public new string name;

    public int maxHP;
    public int attack;

    public Color color;
    public Sprite sprite;

    public Element element;
    [SerializeReference, SubclassSelector]
    public List<IBattleMoveAction> moves = new();

    public AnimatorController unitAnimator;
}
