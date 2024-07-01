using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Needs to attach Health UI and manage that. Functions being public to allow any script to deal damage if needed
/// </summary>

public class Health : MonoBehaviour
{
    [SerializeField] HealthBarController hBC;
    BaseUnit unit;
    int health;
    private void Awake()
    {
        // Attach the health bar to anything that has the health component.
        GameObject uIChild = (GameObject)Resources.Load("UI/HealthBarUI");
        hBC = Instantiate(uIChild, gameObject.transform).GetComponent<HealthBarController>();

        BaseBattleUnit bBU = GetComponent<BaseBattleUnit>();
        bBU.OnUnitCreated += SetUpHealth;
        bBU.OnAlterHealth += HandleHealthChanges;

    }

    void SetUpHealth(BaseUnit unitData)
    {
        unit = unitData;
        health = unitData.maxHP;
        hBC.UpdateUI(health, unit.maxHP);
    }

    /// <summary>
    /// This handles changes in damage. The bool if it is false handles damage to the value whereas if it is true, it handles healing said value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isHealing"></param>
    void HandleHealthChanges(int value, bool isHealing)
    {
        switch (isHealing) {
            // Handling damage
            case false:
                health -= value;
                // If health reduces to less than 0, prevent it
                if(health < 0)
                {
                    health = 0;
                }
                break;

            // Handling healing
            case true:
                health += value;
                // If health exceeds the max health of the unit, cap it at the max health
                if(health > unit.maxHP)
                {
                    health = unit.maxHP;
                }
                break;
        }
        // After all changes, apply the UI
        hBC.UpdateUI(health, unit.maxHP); // Ordinarally, I'd like to also attach this to the event OnAlterHealth but there
                                          // isn't a clean way to do so while also passing the maxHP so having it be a function should be susficient
    }

}
