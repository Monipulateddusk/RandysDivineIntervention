namespace TurnBased.AttackResolution
{
    public class UnitInformation
    {
        public int UnitCurrentHealth { get; }
        public int UnitMaxHealth { get; }
        public int UnitAttack { get; }
        public int UnitSpeed { get; }
        public UnitTeam Team { get; }
        public Element UnitElement { get; }
        public UnitIndex UnitIndex { get; }

        public UnitInformation(int unitCurrentHealth, int unitMaxHealth, int unitAttack, int unitSpeed, UnitTeam team, Element unitElement, UnitIndex unitIndex)
        {
            this.UnitCurrentHealth = unitCurrentHealth;
            this.UnitMaxHealth = unitMaxHealth;
            this.UnitAttack = unitAttack;
            this.UnitSpeed = unitSpeed;
            this.Team = team;
            this.UnitElement = unitElement;
            this.UnitIndex = unitIndex;
        }
    }

    /// <summary>
    /// Should contain resolved information about facets of the field (All unit current health, status, environmental imbued effects, etc).
    /// </summary>
    public class ResolutionSceneData
    {
        public UnitInformation OwnerUnitInformation { get; }
        public System.Collections.Generic.List<UnitInformation> AllyUnitInformation { get; }
        public System.Collections.Generic.List<UnitInformation> EnemyUnitInformation { get; }
        public System.Collections.Generic.List<Elements.ImbuedEnvironmentElement> EnvironmentEffects { get; }

        public ResolutionSceneData(
            UnitInformation sourceUnitInfo, 
            System.Collections.Generic.List<UnitInformation> allyUnitInfo, 
            System.Collections.Generic.List<UnitInformation> enemyUnitInfo, 
            System.Collections.Generic.List<Elements.ImbuedEnvironmentElement> environmentEffects
            )
        {
            this.OwnerUnitInformation   = sourceUnitInfo;
            this.AllyUnitInformation    = allyUnitInfo;
            this.EnemyUnitInformation   = enemyUnitInfo;
            this.EnvironmentEffects     = environmentEffects;
        }
    }
}