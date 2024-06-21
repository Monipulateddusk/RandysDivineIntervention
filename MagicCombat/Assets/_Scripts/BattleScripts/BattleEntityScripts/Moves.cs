using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffect {
    NULL,
}
[System.Serializable]
public class AttackAction
{
    public enum ActionType { DAMAGE, HEALING, STATUS_EFFECT, IMBUE_ENVIRONMENTS}

    public ActionType Type { get; private set; }
    public int Value { get; private set; }
    public StatusEffect StaEffect { get; private set; }
    public Element ElementEffect{  get; private set; } 

    public AttackAction(ActionType type, int value = 0, StatusEffect staEffect = 0, Element elementEff = 0)
    {
        Type = type;
        Value = value;
        StaEffect = staEffect;
        ElementEffect = elementEff;
    }
}

[System.Serializable]
public class AttackResolutionInfo
{
    public List<AttackAction> actions { get; private set; }

    public AttackResolutionInfo()
    {
        actions = new List<AttackAction>();
    }
}
[System.Serializable]
public abstract class BattleMoveAction : ScriptableObject
{
    public AttackResolutionInfo resolutionInfo = new AttackResolutionInfo();
    public abstract AttackResolutionInfo DoMove(BaseUnit userInfo, BaseUnit targetInfo);
}

