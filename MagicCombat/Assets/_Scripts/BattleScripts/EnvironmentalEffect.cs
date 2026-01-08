using System.Collections;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect Name", menuName = "EnvironmentalEffect")]
// Originally wanted to make use of dictonaries but was unable to make them serialisable without a whole new data type creation which isn't worth it.
public class EnvironmentalEffect : ScriptableObject
{
    public List<EnvironmentalEffect> environmentalElements;
    public List<IBattleMoveAction> moves;

    // This function will be used to compare the index of the environmentalElements list and relate that to a list of moves.
    // The returned move will then be processed elsewhere but will need to be a different data type to the BattleMoveAction as I need to give in multiple targets and users
    public IBattleMoveAction ConvertToMove(EnvironmentalEffect effect)
    {
        int index = environmentalElements.IndexOf(effect);
        return moves[index];
    }
}

