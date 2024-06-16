using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This component handles the assignment of the sprite based on the BaseUnit data
/// </summary>
public class SpriteComponent : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<BaseBattleUnit>().OnUnitCreated += AssignSprite;
    }

    void AssignSprite(BaseUnit unit)
    {
        Debug.Log("Assigning sprite data");
        spriteRenderer.sprite = unit.sprite;
        spriteRenderer.color = unit.color;
    }

}
