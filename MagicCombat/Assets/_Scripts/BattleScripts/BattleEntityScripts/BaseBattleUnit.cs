using System;
using UnityEngine;

public enum UnitTeam { ALLY, ENEMY};

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class BaseBattleUnit : MonoBehaviour
{

    // This event handles damage and healing. If the bool is true, then we handle healing, if false, we are taking damage
    public event Action<int, bool> OnAlterHealth;

    [Header("Debugging")]
    [SerializeField] UnitData unitData;
    [SerializeField] Animator unitAnimator;
    [SerializeField] SpriteRenderer unitSpriteRenderer;
    [SerializeField] UnitTeam team;

    /*  Custom Components for the Unit. Using Dependency Injection  */
    BaseMoveSelectorComponent unitInputManagerComponent;
    HealthComponent unitHealthComponent;
    SpriteComponent unitSpriteComponent;
    CombatComponent unitCombatComponent;

    protected ICombatMediator concreteMediator;

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
        unitInputManagerComponent = new SequentialMoveSelectorComponent(this, unitData);
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

    public void NotifyMediator(string ev)
    {
        concreteMediator?.Notify(this, ev);
    }

    #region Animation Methods


    /// <summary>
    /// Play the Animation within the Animation Node correlating to the Attack Name
    /// </summary>
    public void PlayCombatAttackAnimation()
    {
        if (unitAnimator != null)
        {
            unitAnimator.Play(unitCombatComponent.GetCurrentAttackInformation().moveName);
        }
    }


    /// <summary>
    /// ANIMATION EVENT: Called when an Attack Animation Event triggers to do the Attack Action. Process that part of the Attack
    /// </summary>
    public void OnAttackActionAnimationTrigger()
    {
        NotifyMediator("Attack");
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
    public BaseMoveSelectorComponent GetInputManagerComponent() { return unitInputManagerComponent; }
    public HealthComponent GetHealthComponent() { return unitHealthComponent; }
    public SpriteComponent GetSpriteComponent() {  return unitSpriteComponent; }
    public CombatComponent GetCombatComponent() { return unitCombatComponent; }
    public void SetTeam(UnitTeam team) { this.team = team; }
    public UnitTeam GetTeam() { return team; }
    public void SetMediator(ICombatMediator mediator) { concreteMediator = mediator; }

    #endregion
}
