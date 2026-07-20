using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
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

        /*  Get Unity Components and Attach them    */
        if(TryGetComponent(out SpriteRenderer spriteRenderer) && TryGetComponent(out Animator animator))
        {
            this.unitSpriteRenderer = spriteRenderer;
            this.unitAnimator = animator;
            this.unitAnimator.runtimeAnimatorController = unitData.unitAnimator;
        }
        else
        {
            Debug.LogError("UNABLE TO RETRIEVE UNITY COMPONENTS ON: " + gameObject.name);
            Debug.Break();
        }

        /*  Gain a referance to the required components for a Unit.   */
        this.unitSpriteComponent = new SpriteComponent(this, unitData, this.unitSpriteRenderer);
        this.unitData = unitData;
        this.team = team;
    }

    public void OnAnimationEventTriggered(string eventTriggered)
    {

    }

    #region Getter/Setter Methods

    public UnitData GetBaseUnit() { return this.unitData; }
    public SpriteComponent GetSpriteComponent() {  return this.unitSpriteComponent; }
    public UnitTeam GetTeam() { return this.team; }

    #endregion
}
