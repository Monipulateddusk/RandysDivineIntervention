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

    public void Initialise(UnitData unitData)
    {
        if (this.unitData != null) { return; }

        this.unitData = unitData;

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
        unitSpriteComponent = new SpriteComponent(this, unitData, this.unitSpriteRenderer);
    }

    #region Getter/Setter Methods
    public UnitData GetBaseUnit() { return unitData; }
    public SpriteComponent GetSpriteComponent() {  return unitSpriteComponent; }
    public void SetTeam(UnitTeam team) { this.team = team; }
    public UnitTeam GetTeam() { return team; }

    #endregion
}
