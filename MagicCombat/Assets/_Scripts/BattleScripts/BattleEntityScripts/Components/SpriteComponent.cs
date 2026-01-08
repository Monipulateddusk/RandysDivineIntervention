using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component handles the assignment of the sprite based on the BaseUnit data
/// </summary>
public class SpriteComponent : BaseComponent
{

    SpriteRenderer spriteRenderer;

    public SpriteComponent()
    {
        spriteRenderer = null;
    }

    public SpriteComponent(BaseBattleUnit battleUnit, BaseUnit unitData, SpriteRenderer spriteRenderer)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
        this.spriteRenderer = spriteRenderer;

        /*  Assign the Sprite to the Sprite Renderer    */
        spriteRenderer.sprite = unitData.sprite;
        spriteRenderer.color = unitData.color;
    }
}
