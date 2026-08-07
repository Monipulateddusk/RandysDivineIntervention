using UnityEngine;
using UnityEngine.UIElements;

namespace TurnBased.Presentation
{
    public class EnvironmentPresentationManager
    {
        private class EnvironmentPresentationData
        {
            public SummoningCircleVisualHandler Handler {  get; private set; }
            public UnitTeam Team { get; private set; }
            public UnityEngine.Vector3 Position { get; private set; }   
            public Element Element { get; private set; }

            public EnvironmentPresentationData(SummoningCircleVisualHandler handler, UnitTeam unitTeam, UnityEngine.Vector3 pos, Element element)
            {
                this.Handler = handler;
                this.Team = unitTeam;
                this.Position = pos;
                this.Element = element;
            }
        }

        private ParticlesCollection_SO _ParticlesData;

        private System.Collections.Generic.List<EnvironmentPresentationData> instancatedImbuementSummoningCircles;

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

        public async System.Threading.Tasks.Task CreateImbuementSummoningCircleFromUnit(Intention.ResolvingSource source, AttackResolution.AttackAction action)
        {
            if (source.Type != DamageOriginType.UnitMove) { return; }
            if (!StationManager.Instance.TryGetTeamOfUnitIndex(source.SourceUnitIndex, out UnitTeam team)) {  return; }
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(source.SourceUnitIndex, out BaseBattleUnit unit)) {  return; }

            // To get here, we had to have been doing an imbuement attack action. Convert it as such
            AttackResolution.ImbueEnvironmentAttackAction imbueElementAction = (AttackResolution.ImbueEnvironmentAttackAction)action;

            // If there are already some summoning circles of this team, that means we are presenting the combination of the environmental summoning circles.
            if (GetDataOfTeam(team, out System.Collections.Generic.List<EnvironmentPresentationData> dataInTeam))
            {
                await SpawnCombiningSummoningCircleAndMoveCombine(dataInTeam, unit.transform.position, team, imbueElementAction);
            }
            // Otherwise, there are no summoning circles of this team and so we want to instanciate them with that element
            else
            {
                await SpawnSummoningCirclesOfElementAndMoveToPositions(unit.transform.position, team, imbueElementAction);
            }
        }

        private async System.Threading.Tasks.Task SpawnSummoningCirclesOfElementAndMoveToPositions(UnityEngine.Vector3 sourcePosition, UnitTeam team, AttackResolution.ImbueEnvironmentAttackAction imbueElementAction)
        {
            // Get where the summoning circles will be positioned based on which team is imbueing the environment
            System.Collections.Generic.List<UnityEngine.Vector3> positions = GetPositionsForTeam(team);

            // Only go further if there is an element that is to be imbued.
            SummoningCircleImbutentData imbutentData = GetImbuementDataOfElement(imbueElementAction.ElementEffect);
            if (imbutentData == null) { return; }

            float duration = 0;
            // Instanciate two sets of imbuement circles at these positions
            foreach (UnityEngine.Vector3 position in positions)
            {
                if (TryInstanciateSummoningCircleAtSourcePosition(team, sourcePosition, imbueElementAction, out SummoningCircleVisualHandler visualHandler, out EnvironmentPresentationData data)) { continue; }
                _ = visualHandler.RunSummoningCircleVisual(imbutentData);
                _ = visualHandler.MoveSummoningCircleToPosition(position);
                visualHandler.TryGetTotalDuration(out duration);
            }

            await System.Threading.Tasks.Task.Delay((int)(duration * 1000));
        }

        private async System.Threading.Tasks.Task SpawnCombiningSummoningCircleAndMoveCombine(
            System.Collections.Generic.List<EnvironmentPresentationData> existingSummoningCircles, 
            UnityEngine.Vector3 sourcePosition, UnitTeam team, AttackResolution.ImbueEnvironmentAttackAction imbueElementAction)
        {
            // Only go further if there is an element that is to be imbued.
            SummoningCircleImbutentData imbutentData = GetImbuementDataOfElement(imbueElementAction.ElementEffect);
            if (imbutentData == null) { return; }

            if (TryInstanciateSummoningCircleAtSourcePosition(team, sourcePosition, imbueElementAction, out SummoningCircleVisualHandler visualHandler, out EnvironmentPresentationData data)) {  return; }
            await visualHandler.RunSummoningCircleVisual(imbutentData);

            // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            // Move Summoning Circles to the source position.
            //-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            // Get the duration of the Movement
            float duration = 0;
            if (existingSummoningCircles.Count > 0)
            {
                existingSummoningCircles[0].Handler.TryGetTotalDuration(out duration);
            }
            foreach (EnvironmentPresentationData circleData in existingSummoningCircles)
            {
                _ = circleData.Handler.MoveSummoningCircleToPosition(sourcePosition);
            }

            await BlendExistingSummoningCircles(existingSummoningCircles, data, duration);
        }

        private bool TryInstanciateSummoningCircleAtSourcePosition(UnitTeam team, UnityEngine.Vector3 sourcePosition, AttackResolution.ImbueEnvironmentAttackAction imbueElementAction, out SummoningCircleVisualHandler visualHandler, out EnvironmentPresentationData data)
        {
            visualHandler = default;
            data = default; 

            // Instanciate one summoning circle of this new imbued type. Play its animation, then after its done, move the prior circles to the source position for combining.
            UnityEngine.GameObject gO = UnityEngine.GameObject.Instantiate(this._ParticlesData.summoningCircleImbuementPrefab.gameObject);
            if (gO == null || !gO.TryGetComponent(out visualHandler)) { return false; }

            data = new(visualHandler, team, sourcePosition, imbueElementAction.ElementEffect);
            this.instancatedImbuementSummoningCircles.Add(data);
            gO.transform.SetPositionAndRotation(sourcePosition, UnityEngine.Quaternion.identity);
            return true;
        }

        private async System.Threading.Tasks.Task BlendExistingSummoningCircles(System.Collections.Generic.List<EnvironmentPresentationData> existingSummoningCircles, EnvironmentPresentationData otherColourSummoningCircleData, float duration)
        {
            existingSummoningCircles.Add(otherColourSummoningCircleData);

            float startTime = Time.time;
            UnityEngine.Color blendedPrimaryColor = GetBlendedColour(existingSummoningCircles[0].Handler.GetPrimaryColour(), otherColourSummoningCircleData.Handler.GetPrimaryColour());
            UnityEngine.Color blendedSecondaryColor = GetBlendedColour(existingSummoningCircles[0].Handler.GetSecondaryColour(), otherColourSummoningCircleData.Handler.GetSecondaryColour());
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;

                for (int i = 0; i < existingSummoningCircles.Count; i++)
                {
                    UnityEngine.Color primary = existingSummoningCircles[i].Handler.GetPrimaryColour();
                    UnityEngine.Color secondary = existingSummoningCircles[i].Handler.GetSecondaryColour();
                    UnityEngine.Color newPrimaryColour = new(Mathf.SmoothStep(primary.r, blendedPrimaryColor.r, t), Mathf.SmoothStep(primary.g, blendedPrimaryColor.g, t), Mathf.SmoothStep(primary.b, blendedPrimaryColor.b, t));
                    UnityEngine.Color newSecondaryColour = new(Mathf.SmoothStep(secondary.r, blendedSecondaryColor.r, t), Mathf.SmoothStep(secondary.g, blendedSecondaryColor.g, t), Mathf.SmoothStep(secondary.b, blendedSecondaryColor.b, t));

                    existingSummoningCircles[i].Handler.ChangeMaterialColour(newPrimaryColour, newSecondaryColour);
                }
                await System.Threading.Tasks.Task.Yield();
            }

            await System.Threading.Tasks.Task.Delay((int)(1500));
        }

        private void DestroyAllInstanciatedSummoningCircles()
        {
            int length = this.instancatedImbuementSummoningCircles.Count;
            for (int i = 0; i < length; i++)
            {
                UnityEngine.GameObject.Destroy(this.instancatedImbuementSummoningCircles[i].Handler.gameObject);
            }

            this.instancatedImbuementSummoningCircles = null;
        }

        private SummoningCircleImbutentData GetImbuementDataOfElement(Element element)
        {
            switch (element)
            {
                case Element.DARKNESS:
                    return this._ParticlesData.darkImbuementCircleData;
                case Element.LIGHT:
                    return this._ParticlesData.lightImbuementCircleData;
                case Element.WATER:
                    return this._ParticlesData.waterImbuementCircleData;
                case Element.EARTH:
                    return this._ParticlesData.earthImbuementCircleData;
                case Element.FIRE:
                    return this._ParticlesData.fireImbuementCircleData;
                case Element.ICE:
                    return this._ParticlesData.iceImbuementCircleData;
                default:
                    return null;
            }
        }

        private System.Collections.Generic.List<UnityEngine.Vector3> GetPositionsForTeam(UnitTeam team) =>  team == UnitTeam.ALLY ? IMBUEMENT_CIRCLE_POSITIONS_ALLY : IMBUEMENT_CIRCLE_POSITIONS_ENEMY;
        private bool GetDataOfTeam(UnitTeam team, out System.Collections.Generic.List<EnvironmentPresentationData> dataOfTeam)
        {
            dataOfTeam = new();
            foreach (EnvironmentPresentationData dataIndex in this.instancatedImbuementSummoningCircles)
            {
                if (dataIndex.Team == team)
                {
                    dataOfTeam.Add(dataIndex);
                }
            }

            return dataOfTeam.Count > 0;    
        }

        private UnityEngine.Color GetBlendedColour(UnityEngine.Color colourA, UnityEngine.Color colourB) 
        {
            float r = (colourA.r + colourB.r) / 2;
            float g = (colourA.g + colourB.g) / 2;
            float b = (colourA.b + colourB.b) / 2;

            return new(r, g, b, 1);
        }
    }
}