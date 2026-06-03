using System.Linq;

namespace TurnBased.Elements
{
    public struct ImbuedEnvironmentElement
    {
        public UnitTeam team;
        public Element imbuedEnvironmentElement;

        public ImbuedEnvironmentElement(UnitTeam team, Element element)
        {
            this.team = team;
            this.imbuedEnvironmentElement = element;
        }
    }

    public class CombatEnvironmentController
    {
        private static CombatEnvironmentController instance;
        public static CombatEnvironmentController Instance
        {
            get
            {
                return instance;
            }
            private set
            {
                instance = value;                
            }
        }

        public static event System.Action<ImbuedEnvironmentElement> OnAddElementalImbuement;
        public static event System.Action<ImbuedEnvironmentElement> OnRemoveElementalImbuement;


        /*  Elemental Look-Up Table     */
        /*  Same size as the Element Enum. Units are able to imbue the Environment with their Element to do an attack if their Ally participates.   */
        private readonly IElementalMove[,] ElementalMoveLookUpTable = new IElementalMove[7, 7]
        {   /*  NULL,   Fire                Water                   Ice                     Earth                       Light                   Darkness    */
            {   null,   null,               null,                   null,                   null,                       null,                   null,  },   /* NULL     */
            {   null,   new EM_Inferno(),   new EM_Steam(),         new EM_Frostburn(),     new EM_Volcano(),           null,                   null,  },   /* Fire     */
            {   null,   new EM_Steam(),     new EM_Tsunami(),       new EM_HailCloak(),     new EM_Wellspring(),        null,                   null,  },   /* Water    */
            {   null,   new EM_Frostburn(), new EM_HailCloak(),     new EM_IceAge(),        new EM_FrostLock(),         null,                   null,  },   /* Ice      */
            {   null,   new EM_Volcano(),   new EM_Wellspring(),    new EM_FrostLock(),     new EM_Fissure(),           null,                   null,  },   /* Earth    */
            {   null,   null,               null,                   null,                   null,                       null,                   null,  },   /* Light    */
            {   null,   null,               null,                   null,                   null,                       null,                   null,  }    /* Darkness */
        };

        private System.Collections.Generic.List<ImbuedEnvironmentElement> EnvironmentEffects;
        private GeneralPurposeTargettingManager GeneralPurposeTargettingManager;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.EnvironmentEffects = new();

            /*  Initalise the Targetting Manager for the moves. */
            this.GeneralPurposeTargettingManager = new();
            this.GeneralPurposeTargettingManager.Awake(this.ElementalMoveLookUpTable);
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }
            this.EnvironmentEffects.Clear();

            this.GeneralPurposeTargettingManager.OnDestroy();
        }

        public void AddEnvironmentalEffect(Element effect, UnitIndex unitIndex)
        {
            UnityEngine.Debug.LogError($"Adding element: {effect}");

            /*  Add the imbued element to the list and alert any listeners. */
            UnitTeam team = StationManager.Instance.GetUnitTeamOfIndex(unitIndex);
            ImbuedEnvironmentElement imbuedEnvironmentElement = new(team, effect);
            this.EnvironmentEffects.Add(imbuedEnvironmentElement);
            OnAddElementalImbuement?.Invoke(imbuedEnvironmentElement);

            DetermineIfProcessingElementalMove(unitIndex, team);
        }

        private void DetermineIfProcessingElementalMove(UnitIndex unitIndex, UnitTeam team)
        {
            /*  Check the List. Does it contain two entries on the same team? If so, process that corresponding elemental attack.   */
            System.Collections.Generic.Queue<ImbuedEnvironmentElement> pairedElementsForElementalAttack = new(this.EnvironmentEffects.Where(iEE => iEE.team == team));
            if (pairedElementsForElementalAttack.Count >= 2)
            {
                RemovePairedElementsForAttack(pairedElementsForElementalAttack);


                /*  Get the two elements to process it. */
                Element elementA = pairedElementsForElementalAttack.Dequeue().imbuedEnvironmentElement;
                Element elementB = pairedElementsForElementalAttack.Dequeue().imbuedEnvironmentElement;

                UnityEngine.Debug.LogError($"Processing paired elements: Element A: {elementA}, Element B: {elementB}");

                ProcessElementalMove(elementA, elementB, unitIndex, team);
            }
        }

        private void RemovePairedElementsForAttack(System.Collections.Generic.Queue<ImbuedEnvironmentElement> pairedElementsForElementalAttack)
        {
            for (int i = 0; i < pairedElementsForElementalAttack.Count; i++)
            {
                /*  Remove both imbued Elements on the team and alert any listeners.    */
                ImbuedEnvironmentElement removedImbuedEnvironmentElement = pairedElementsForElementalAttack.ToList()[i];
                this.EnvironmentEffects.Remove(removedImbuedEnvironmentElement);
                OnRemoveElementalImbuement?.Invoke(removedImbuedEnvironmentElement);
            }
        }

        private void ProcessElementalMove(Element elementValueA, Element elementValueB, UnitIndex unitIndex, UnitTeam team)
        {
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForElementalMove(team, out TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData)) { return; }
            if (!StationManagerUtilities.TryCreateSceneDataForTeam(team, out UnitTurnStationIndexesSceneData stationIndexesSceneData)) { return; }

            //  -=-=-=-=-=-=-=-=-
            //  Process Elemental Attack Move and ensure it isn't null
            //  -=-=-=-=-=-=-=-=-
            IElementalMove elementalAttackMoveAction = GetElementalCombination(elementValueA, elementValueB);

            //  -=-=-=-=-=-=-=-=-
            //  Process Elemental Resolving State and Targets
            //  -=-=-=-=-=-=-=-=-
            Intention.ResolvingSource resolvingSource = new(elementalAttackMoveAction, unitIndex);
            Intention.ResolvingState elementalMoveResolvingState = new(resolvingSource);
            DeclareElementalMoveTargets(unitIndex, resolutionSceneData, stationIndexesSceneData, elementalMoveResolvingState, elementalAttackMoveAction);

            //  -=-=-=-=-=-=-=-=-
            //  Request the adding of Elemental Moves to the Resolver.
            //  -=-=-=-=-=-=-=-=-
            AttackResolution.CombatResolvingRequest resolvingRequest = Combat.IntentionCombatResolverUtility.AddToCombatResolverFront(elementalMoveResolvingState);
        }

        private void DeclareElementalMoveTargets(UnitIndex unitIndex, TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData, UnitTurnStationIndexesSceneData stationIndexesSceneData, Intention.ResolvingState elementalMoveResolvingState, IElementalMove elementalAttackMoveAction)
        {
            //  -=-=-=-=-=-=-=-=-
            //  Execute the elemental move to pass to the Factory the ResolutionInfo
            //  -=-=-=-=-=-=-=-=-
            AttackResolutionInfo info = elementalAttackMoveAction.ExecuteElementalMove(resolutionSceneData);

            /*  Before we process it, we need to determine the Targets of the attack. The ElementalMoveAction will dictate who it targets.  */
            if (!this.GeneralPurposeTargettingManager.TryGetTargetSelector(elementalAttackMoveAction.GetMoveName(), out TargetSelection.ITargetSelector targetSelector)) { return; }

            if (!Intention.UnitIntentionFactory.BuildAttackActionResolvingState(unitIndex, info, elementalMoveResolvingState)) { return; }


            Intention.IntentionResolverManager.Instance.ProcessResolvingStateTargetSelection(stationIndexesSceneData, elementalMoveResolvingState, targetSelector);
        }

        private IElementalMove GetElementalCombination(Element elementValueA, Element elementValueB)
        {
            if ((uint)elementValueA < (uint)ElementalMoveLookUpTable.GetLength(0) &&
                (uint)elementValueB < (uint)ElementalMoveLookUpTable.GetLength(1))
            {
                return ElementalMoveLookUpTable[(uint)elementValueA, (uint)elementValueB];
            }
            return null;
        }

        public System.Collections.Generic.List<ImbuedEnvironmentElement> GetImbuedEnvironmentElements() { return EnvironmentEffects; }
    }
}