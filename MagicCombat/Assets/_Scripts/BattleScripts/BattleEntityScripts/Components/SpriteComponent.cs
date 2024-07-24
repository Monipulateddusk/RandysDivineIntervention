using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component handles the assignment of the sprite based on the BaseUnit data
/// </summary>
public class SpriteComponent : BaseCombatComponent
{
    SpriteRenderer spriteRenderer;

    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        bBU.OnUnitCreated += AssignSprite;
    }
    void AssignSprite(BaseUnit unit)
    {
        //Debug.Log("Assigning sprite data");
        spriteRenderer.sprite = unit.sprite;
        spriteRenderer.color = unit.color;
    }


}
