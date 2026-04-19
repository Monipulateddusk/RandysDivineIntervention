using System;
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
    CombatComponent unitCombatComponent;

    private void Awake()
    {
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
        unitCombatComponent = new CombatComponent(this, unitData, gameObject.transform);


    }


    #region Animation Methods


    /// <summary>
    /// Play the Animation within the Animation Node correlating to the Attack Name
    /// </summary>
    public void PlayCombatAttackAnimation()
    {
        if (unitAnimator != null)
        {
           // unitAnimator.Play(unitCombatComponent.GetCurrentAttackInformation().moveName);
        }
    }


    /// <summary>
    /// ANIMATION EVENT: Called when an Attack Animation Event triggers to do the Attack Action. Process that part of the Attack
    /// </summary>
    public void OnAttackActionAnimationTrigger()
    {
       
    }


    /// <summary>
    /// ANIMATION EVENT: Called when the End Attack Animation Event is triggered. 
    /// </summary>
    public async void OnAttackAnimationEnd()
    {
        await GetCombatComponent().OnEndAttackAnimation();
    }


    #endregion

    #region Getter/Setter Methods

    public UnitData GetBaseUnit() { return unitData; }
    public SpriteComponent GetSpriteComponent() {  return unitSpriteComponent; }
    public CombatComponent GetCombatComponent() { return unitCombatComponent; }
    public void SetTeam(UnitTeam team) { this.team = team; }
    public UnitTeam GetTeam() { return team; }

    #endregion
}
