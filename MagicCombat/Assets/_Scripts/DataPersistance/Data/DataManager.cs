using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour, IDataPersistance
{
    [SerializeField] List<PlayerCharacter> playerChars = new List<PlayerCharacter>();

    public void LoadData(GameData data)
    {
        playerChars = data.playerCharacters;
    }

    public void SaveData(ref GameData data)
    {
         data.playerCharacters = playerChars;
    }
}
