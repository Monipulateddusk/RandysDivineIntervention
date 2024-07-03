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

    [SerializeField]InputManager iMComponent;
    [SerializeField] Health hComponent;
    [SerializeField] SpriteComponent sComponent;

    private void Awake()
    {
        // Attach the required component for a Unit. I wonder if there is a better way to do this
        iMComponent = InputManager.CreateInstance(gameObject, this);
        hComponent = gameObject.AddComponent<Health>();
        sComponent = gameObject.AddComponent<SpriteComponent>();
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
    public InputManager GetInputManagerComponent() { return iMComponent; }
    public Health GetHealthComponent() { return hComponent; }
    public SpriteComponent GetSpriteComponent() {  return sComponent; }

    #endregion

    /// <summary>
    /// Effectively similar to a deconstructor. Needed to unsubscribe to events to prevent memory leeks
    /// </summary>
    private void OnDestroy()
    {
        OnUnitCreated = null;
        OnAlterHealth = null;
    }
}
