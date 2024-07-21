using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBattleUnit : MonoBehaviour
{
    // This event handles creation of the object. While this class handles the logistics of data, other classes handle the visual elements and thus need BaseUnit data to assign values correctly.
    public event Action<BaseUnit> OnUnitCreated;

    // This event handles damage and healing. If the bool is true, then we handle healing, if false, we are taking damage
    public event Action<int, bool> OnAlterHealth;

    [Header("Debugging")]
    [SerializeField] BaseUnit unitData;

    [SerializeField] BaseInputManagerComponent iMComponent;
    [SerializeField] Health hComponent;
    [SerializeField] SpriteComponent sComponent;
    [SerializeField] AnimationControllerComponent aCComponent;
    [SerializeField] CombatComponent cComponent;

    private void Awake()
    {
        // Attach the required component for a Unit.
        iMComponent = BaseComponent.CreateInstance<SequentialInputManagerComponent, BaseBattleUnit>(gameObject, this);
        hComponent = BaseComponent.CreateInstance<Health, BaseBattleUnit>(gameObject, this);
        sComponent = BaseComponent.CreateInstance<SpriteComponent, BaseBattleUnit>(gameObject, this);
        aCComponent = BaseComponent.CreateInstance<AnimationControllerComponent, BaseBattleUnit>(gameObject, this);
        cComponent = BaseComponent.CreateInstance<CombatComponent, BaseBattleUnit>(gameObject, this);
    }

    private void OnEnable()
    {
        OnUnitCreated += AssignUnitData;
        OnUnitCreated?.Invoke(unitData);
    }

    void AssignUnitData(BaseUnit unit)
    {
        Debug.Log("Assigning Unit Data");
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
        if(Input.GetKeyUp(KeyCode.D))
        {
            Damage(1);
        }
        if(Input.GetKeyUp(KeyCode.H))
        { 
            Heal(1); 
        }
    }

    #region Getters
    public  BaseUnit GetBaseUnit() { return unitData; }
    public BaseInputManagerComponent GetInputManagerComponent() { return iMComponent; }
    public Health GetHealthComponent() { return hComponent; }
    public SpriteComponent GetSpriteComponent() {  return sComponent; }
    public AnimationControllerComponent  GetAnimationControllerComponent() { return aCComponent; }
    public CombatComponent GetCombatComponent() { return cComponent; }

    #endregion

    /// <summary>
    /// Effectively similar to a deconstructor. Needed to unsubscribe to events to prevent memory leaks
    /// </summary>
    private void OnDestroy()
    {
        OnUnitCreated = null;
        OnAlterHealth = null;
    }
}
