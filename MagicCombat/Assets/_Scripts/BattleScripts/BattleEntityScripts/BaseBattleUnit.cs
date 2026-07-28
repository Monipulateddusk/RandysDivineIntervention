using UnityEngine;

public class BaseBattleUnit : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] UnitData unitData;
    [SerializeField] Animator unitAnimator;
    [SerializeField] SpriteRenderer unitSpriteRenderer;
    [SerializeField] UnitTeam team;

    /*  Custom Components for the Unit. Using Dependency Injection  */
    SpriteComponent unitSpriteComponent;

    public void Initialise(UnitData unitData, UnitTeam team)
    {
        /*  Don't initalise the unit if it already has unitData.    */
        if (this.unitData != null) { return; }

        this.unitData = unitData;
        this.team = team;


        /*  Create a child GameObject that handles visuals. This way we can rotate the Unit in accordance to the Camera, and have full rotation movement via Animations.    */
        GameObject childGameObject = new($"{this.unitData.name} Visuals");

        /*  Add the Sprite Render and Animator to the Child, and attach it to this parent GameObject.   */
        childGameObject.AddComponent<BattleUnitAnimationHandler>();

        this.unitSpriteRenderer = childGameObject.AddComponent<SpriteRenderer>();
        this.unitAnimator = childGameObject.AddComponent<Animator>();
        this.unitAnimator.runtimeAnimatorController = unitData.unitAnimator;

        childGameObject.transform.parent = this.gameObject.transform;

        /*  Gain a referance to the required components for a Unit.   */
        this.unitSpriteComponent = new SpriteComponent(this, unitData, this.unitSpriteRenderer);

    }

    #region Getter/Setter Methods

    public UnitData GetBaseUnit() { return this.unitData; }
    public SpriteComponent GetSpriteComponent() {  return this.unitSpriteComponent; }
    public Animator GetAnimator() { return this.unitAnimator; }
    public UnitTeam GetTeam() { return this.team; }

    #endregion
}
