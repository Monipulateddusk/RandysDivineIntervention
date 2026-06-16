namespace TurnBased.Information
{
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