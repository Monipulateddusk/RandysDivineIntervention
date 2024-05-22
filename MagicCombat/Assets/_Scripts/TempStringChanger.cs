using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TempStringChanger : MonoBehaviour, IDataPersistance
{
    public string text;

    public TextMeshProUGUI textMeshProUGUI;
    public void LoadData(GameData data)
    {
        this.text = data.textInBox;
    }

    public void SaveData(ref GameData data)
    {
        data.textInBox = this.text;
    }

    private void Update()
    {
        textMeshProUGUI.text = text;

    }
}
