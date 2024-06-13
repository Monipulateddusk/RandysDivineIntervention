using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HealthBarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Image healthBarImage;

    [SerializeField]uint curHealth, maxHealth;

    private void OnValidate()
    {
        if (curHealth >= maxHealth)
        {
            curHealth = maxHealth;
        }
        UpdateUI();
   
    }
    public void UpdateUI()
    {
        healthBarImage.fillAmount = (float)curHealth / (float)maxHealth;
        healthText.text = curHealth.ToString() + "/" + maxHealth.ToString();
    }
}
