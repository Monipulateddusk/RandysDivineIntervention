using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HealthBarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Image healthBarImage;

    /// <summary>
    /// Called in other classes to update UI based on passed in values of HP for that Unit
    /// </summary>
    /// <param name="curHealth"></param>
    /// <param name="maxHealth"></param>
    public void UpdateUI(float curHealth, float maxHealth)
    {
        healthBarImage.fillAmount = (float)curHealth / (float)maxHealth;
        healthText.text = curHealth.ToString() + "/" + maxHealth.ToString();
    }
}
