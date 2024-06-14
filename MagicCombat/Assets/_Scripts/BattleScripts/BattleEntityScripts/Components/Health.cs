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

    private void Awake()
    {
        takeDMG += UpdateHealth;
        GameObject uIChild = (GameObject)Resources.Load("UI/HealthBarUI");
        Instantiate(uIChild, gameObject.transform);
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateHealth()
    {
        Debug.Log("Taking Damage");
    }
}
