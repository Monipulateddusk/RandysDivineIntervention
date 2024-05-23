using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string textInBox;

    public List<PlayerCharacter> playerCharacters;
    /// <summary>
    /// Public Constructor containing default values
    /// </summary>
    public GameData()
    {
        textInBox = "";
        playerCharacters = new List<PlayerCharacter>();
    }
}
