using UnityEngine;

[System.Serializable]
public class PlayerCharacter
{
    public enum Class
    {
        BARBARIAN, WIZARD, PALADIN, ROGUE, FIGHTER,
    }
    public enum MagicElement
    {
        FIRE, ICE, WATER, EARTH, LIGHT, DARKNESS,
    }

    [Range(0,100)] public int level, physAtk, magiAtk, physDef, magiDef, health, speed;

    public Class classType;
    public MagicElement magicElement;

    public Color primaryColour, secondaryColour;
}
