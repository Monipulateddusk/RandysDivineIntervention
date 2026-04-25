using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteComponent : BaseComponent
{

    SpriteRenderer spriteRenderer;

    public SpriteComponent()
    {
        spriteRenderer = null;
    }

    public SpriteComponent(BaseBattleUnit battleUnit, UnitData unitData, SpriteRenderer spriteRenderer)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
        this.spriteRenderer = spriteRenderer;

        /*  Assign the Sprite to the Sprite Renderer    */
        spriteRenderer.sprite = unitData.sprite;
        spriteRenderer.color = unitData.color;
    }

    public void SetSpriteRenderTransparency(float transparencyValue)
    {
        this.spriteRenderer.color = 
            new Color(
            this.spriteRenderer.color.r, 
            this.spriteRenderer.color.g, 
            this.spriteRenderer.color.b, 
            transparencyValue);
    }
}
