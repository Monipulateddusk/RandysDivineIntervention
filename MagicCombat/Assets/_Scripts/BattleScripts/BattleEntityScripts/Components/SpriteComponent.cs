using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component handles the assignment of the sprite based on the BaseUnit data
/// </summary>
public class SpriteComponent : BaseComponent
{
    SpriteRenderer spriteRenderer;

    protected override void Initialize<U>(U initParameter)
    {
        if (initParameter is BaseBattleUnit unit)
        {
            bBU = unit;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            bBU.OnUnitCreated += AssignSprite;
        }
    }

    void AssignSprite(BaseUnit unit)
    {
        Debug.Log("Assigning sprite data");
        spriteRenderer.sprite = unit.sprite;
        spriteRenderer.color = unit.color;
    }


}
