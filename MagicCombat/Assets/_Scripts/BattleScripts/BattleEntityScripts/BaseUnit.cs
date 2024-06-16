using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
}
