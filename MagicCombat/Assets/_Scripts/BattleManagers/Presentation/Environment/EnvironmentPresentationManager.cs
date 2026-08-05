namespace TurnBased.Presentation
{
    public class EnvironmentPresentationManager
    {
        private ParticlesCollection_SO _ParticlesData;

        private System.Collections.Generic.List<SummoningCircleVisualHandler> instancatedImbuementSummoningCircles;

        private readonly System.Collections.Generic.List<UnityEngine.Vector3> IMBUEMENT_CIRCLE_POSITIONS_ALLY = new()
        {
            new UnityEngine.Vector3(-5, 0, 3), new UnityEngine.Vector3(6, 0, 1),
        };
        private readonly System.Collections.Generic.List<UnityEngine.Vector3> IMBUEMENT_CIRCLE_POSITIONS_ENEMY = new()
        {
            new UnityEngine.Vector3(-4, 0, -6), new UnityEngine.Vector3(6, 0, -8),
        };

        public void Awake(ParticlesCollection_SO data)
        {
            this._ParticlesData = data;
            this.instancatedImbuementSummoningCircles = new();
        }

        public void OnDestroy()
        {
            DestroyAllInstanciatedSummoningCircles();
        }

        public void CreateImbuementSummoningCircleFromUnit(Intention.ResolvingSource source)
        {
            if (source.Type != DamageOriginType.UnitMove) { return; }
            if (!StationManager.Instance.TryGetTeamOfUnitIndex(source.SourceUnitIndex, out UnitTeam team)) {  return; } 

            System.Collections.Generic.List<UnityEngine.Vector3> positions = GetPositionsForTeam(team);

            // Instanciate two sets of imbuement circles at these positions
            foreach (UnityEngine.Vector3 position in positions)
            {
                UnityEngine.GameObject gO = UnityEngine.GameObject.Instantiate(this._ParticlesData.summoningCircleImbuementPrefab.gameObject);
                if (gO != null && gO.TryGetComponent(out SummoningCircleVisualHandler visualHandler))
                {
                    this.instancatedImbuementSummoningCircles.Add(visualHandler);
                    gO.transform.SetPositionAndRotation(position, UnityEngine.Quaternion.identity);
                    visualHandler.InitaliseSummoningCircleVisual(this._ParticlesData.darkImbuementCircleData);
                }
            }
        }

        private void DestroyAllInstanciatedSummoningCircles()
        {
            int length = this.instancatedImbuementSummoningCircles.Count;
            for (int i = 0; i < length; i++)
            {
                UnityEngine.GameObject.Destroy(this.instancatedImbuementSummoningCircles[i].gameObject);
            }

            this.instancatedImbuementSummoningCircles = null;
        }


        private System.Collections.Generic.List<UnityEngine.Vector3> GetPositionsForTeam(UnitTeam team) =>  team == UnitTeam.ALLY ? IMBUEMENT_CIRCLE_POSITIONS_ALLY : IMBUEMENT_CIRCLE_POSITIONS_ENEMY;
    }
}