using System;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class BaseBattleUnit : MonoBehaviour
{
    // This event handles creation of the object. While this class handles the logistics of data, other classes handle the visual elements and thus need BaseUnit data to assign values correctly.
    public event Action<BaseUnit> OnUnitCreated;

    // This event handles damage and healing. If the bool is true, then we handle healing, if false, we are taking damage
    public event Action<int, bool> OnAlterHealth;

    [Header("Debugging")]
    [SerializeField] BaseUnit unitData;

    [SerializeField] BaseInputManagerComponent iMComponent;
    [SerializeField] HealthComponent hComponent;
    [SerializeField] SpriteComponent sComponent;
    [SerializeField] CombatComponent cComponent;

    [SerializeField] Animator unitAnimator;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void Awake()
    {
        /*  Get Unity Components and Attach them    */
        if(TryGetComponent(out SpriteRenderer spriteRenderer) && TryGetComponent(out Animator animator))
        {
            this.spriteRenderer = spriteRenderer;
            this.unitAnimator = animator;
            this.unitAnimator.runtimeAnimatorController = unitData.unitAnimator;
        }
        else
        {
            Debug.LogError("UNABLE TO RETRIEVE UNITY COMPONENTS ON: " + gameObject.name);
            Debug.Break();
        }

        /*  Gain a referance to the required components for a Unit.   */
        iMComponent = new SequentialInputManagerComponent(this, unitData);
        hComponent = new HealthComponent(this, unitData);
        sComponent = new SpriteComponent(this, unitData, spriteRenderer);
        cComponent = new CombatComponent(this, unitData, gameObject.transform);


    }

    private void OnEnable()
    {
        OnUnitCreated?.Invoke(unitData);
    }

    /// <summary>
    /// Effectively similar to a deconstructor. Needed to unsubscribe to events to prevent memory leaks
    /// </summary>
    private void OnDestroy()
    {
        OnUnitCreated = null;
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
        unitAnimator.Play(cComponent.GetCurrentAttackInformation().moveName);
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

    public BaseUnit GetBaseUnit() { return unitData; }
    public BaseInputManagerComponent GetInputManagerComponent() { return iMComponent; }
    public HealthComponent GetHealthComponent() { return hComponent; }
    public SpriteComponent GetSpriteComponent() {  return sComponent; }
    public CombatComponent GetCombatComponent() { return cComponent; }

    #endregion
}
