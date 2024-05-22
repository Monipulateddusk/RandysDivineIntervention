using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistance 
{
    void LoadData(GameData data);
    
    // Passing by referance to allow implementing script to modify the data, as opposed to the LoadData where it only cares about reading that data
    void SaveData(ref GameData data);
}
