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
    public event Action takeDMG;

   [SerializeField] HealthBarController hBC;

    private void Awake()
    {
        takeDMG += UpdateHealth;

        // Attach the health bar to anything that has the health component.
        GameObject uIChild = (GameObject)Resources.Load("UI/HealthBarUI");
        hBC = Instantiate(uIChild, gameObject.transform).GetComponent<HealthBarController>();
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateHealth()
    {
        Debug.Log("Taking Damage");
    }
}
