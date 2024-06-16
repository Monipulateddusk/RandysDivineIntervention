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

    [SerializeField] BaseUnit unitData;


    private void Start()
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


}
