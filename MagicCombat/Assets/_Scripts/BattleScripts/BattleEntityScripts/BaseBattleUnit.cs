using System;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class BaseBattleUnit : MonoBehaviour
{

    // This event handles damage and healing. If the bool is true, then we handle healing, if false, we are taking damage
    public event Action<int, bool> OnAlterHealth;

    [Header("Debugging")]
    [SerializeField] UnitData unitData;
    [SerializeField] Animator unitAnimator;
    [SerializeField] SpriteRenderer unitSpriteRenderer;

    /*  Custom Components for the Unit. Using Dependency Injection  */
    BaseInputManagerComponent unitInputManagerComponent;
    HealthComponent unitHealthComponent;
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
        unitInputManagerComponent = new SequentialInputManagerComponent(this, unitData);
        unitHealthComponent = new HealthComponent(this, unitData);
        unitSpriteComponent = new SpriteComponent(this, unitData, this.unitSpriteRenderer);
        unitCombatComponent = new CombatComponent(this, unitData, gameObject.transform);


    }

    private void OnDestroy()
    {
        OnAlterHealth = null;
    }

    public void Damage(int damageAmount)
    {
        OnAlterHealth?.Invoke(damageAmount, false);
    }
    public void Heal(int healAmount)
    {
        OnAlterHealth?.Invoke(healAmount, true);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            Damage(1);
        }
        if (Input.GetKeyUp(KeyCode.H))
        {
            Heal(1);
        }
    }

    #region Animation Methods
    

    /// <summary>
    /// Play the Animation within the Animation Node correlating to the Attack Name
    /// </summary>
    public void PlayCombatAttackAnimation()
    {
        unitAnimator.Play(unitCombatComponent.GetCurrentAttackInformation().moveName);
    }


    /// <summary>
    /// ANIMATION EVENT: Called when an Attack Animation Event triggers to do the Attack Action. Process that part of the Attack
    /// </summary>
    public void OnAttackActionAnimationTrigger()
    {
        GetCombatComponent().ProcessAttack();
    }


    /// <summary>
    /// ANIMATION EVENT: Called when the End Attack Animation Event is triggered. 
    /// </summary>
    public async void OnAttackAnimationEnd()
    {
        await GetCombatComponent().OnEndAttackAnimation();
    }


    #endregion

    #region Getter Methods for Components

    public UnitData GetBaseUnit() { return unitData; }
    public BaseInputManagerComponent GetInputManagerComponent() { return unitInputManagerComponent; }
    public HealthComponent GetHealthComponent() { return unitHealthComponent; }
    public SpriteComponent GetSpriteComponent() {  return unitSpriteComponent; }
    public CombatComponent GetCombatComponent() { return unitCombatComponent; }

    #endregion
}
