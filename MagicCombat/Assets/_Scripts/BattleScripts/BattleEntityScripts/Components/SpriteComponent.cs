public class SpriteComponent : BaseComponent
{
    private readonly UnityEngine.SpriteRenderer spriteRenderer;

    public SpriteComponent()
    {
        spriteRenderer = null;
    }

    public SpriteComponent(BaseBattleUnit battleUnit, UnitData unitData, UnityEngine.SpriteRenderer spriteRenderer)
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
        if (this.spriteRenderer != null) { return; }
        
        this.spriteRenderer.color = 
            new UnityEngine.Color(
            this.spriteRenderer.color.r, 
            this.spriteRenderer.color.g, 
            this.spriteRenderer.color.b, 
            transparencyValue);
    }
    public void FlipSpriteRendererX(bool flipValue)
    {
        if (this.spriteRenderer != null) { return; }
        this.spriteRenderer.flipX = flipValue;
    }
}
