using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionary<K, V> : Dictionary<K,V>, ISerializationCallbackReceiver
{
    public List<K> keys = new();
    public List<V> values = new();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach(var keyValuePair in this)
        {
            keys.Add(keyValuePair.Key);
            values.Add(keyValuePair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();
        for(int i = 0; i < keys.Count; i++)
        {
            if(values[i] == null) { Debug.LogError("SERIALIZABLE_DICTIONARY_ERROR: INCORRECT KEY VALUE PAIR"); }
            Add(keys[i], values[i]);
        }

        keys.Clear();
        values.Clear(); 
    }


}
