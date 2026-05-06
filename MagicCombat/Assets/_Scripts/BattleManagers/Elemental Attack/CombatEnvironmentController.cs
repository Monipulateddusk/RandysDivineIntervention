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
        private readonly IElementalMoveAction[,] ElementalMoveLookUpTable = new IElementalMoveAction[7, 7]
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
        private ElementalAttackTargettingManager ElementalAttackTargettingManager;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.EnvironmentEffects = new();

            /*  Initalise the Targetting Manager for the moves. */
            this.ElementalAttackTargettingManager = new();
            this.ElementalAttackTargettingManager.Awake(this.ElementalMoveLookUpTable);
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }
            this.EnvironmentEffects.Clear();

            this.ElementalAttackTargettingManager.OnDestroy();
        }

        public void Update()
        {
            this.ElementalAttackTargettingManager.Update();
        }

        public void AddEnvironmentalEffect(Element effect, UnitTeam team)
        {
            /*  Add the imbued element to the list and alert any listeners. */
            ImbuedEnvironmentElement imbuedEnvironmentElement = new(team, effect);
            this.EnvironmentEffects.Add(imbuedEnvironmentElement);
            OnAddElementalImbuement?.Invoke(imbuedEnvironmentElement);

            DetermineIfProcessingElementalMove(team);
        }

        private void DetermineIfProcessingElementalMove(UnitTeam team)
        {
            /*  Check the List. Does it contain two entries on the same team? If so, process that corresponding elemental attack.   */
            System.Collections.Generic.Queue<ImbuedEnvironmentElement> pairedElementsForElementalAttack = new(this.EnvironmentEffects.Where(iEE => iEE.team == team));
            if (pairedElementsForElementalAttack.Count >= 2)
            {
                RemovePairedElementsForAttack(pairedElementsForElementalAttack);


                /*  Get the two elements to process it. */
                Element elementA = pairedElementsForElementalAttack.Dequeue().imbuedEnvironmentElement;
                Element elementB = pairedElementsForElementalAttack.Dequeue().imbuedEnvironmentElement;
                ProcessElementalMove(elementA, elementB, team);
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

        private void ProcessElementalMove(Element elementValueA, Element elementValueB, UnitTeam team)
        {
            // Find out what move the elements combine into and do that move to get the info needed to resolve it
            IElementalMoveAction elementalAttackMoveAction = GetElementalCombination(elementValueA, elementValueB);
            if (elementalAttackMoveAction == null) { UnityEngine.Debug.LogError("ELEMENTAL COMBINATION ERROR: NOT VALID!!!"); return; }

            /*  Get the first entry of Units on the Team to retrieve the attack information.    */
            System.Collections.Generic.List<UnitIndex> unitsOnTeam = StationManager.Instance.GetUnitsOnTeam(team);
            if (unitsOnTeam.Count <= 0) { return; }

            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForElementalMove(team, out UnitData_SceneData_UnitTurn unitDataSceneData)) { return; }

            /*  Process the attack using the Scene Unit Data.   */
            AttackResolutionInfo elementalAttackResolutionInfo = elementalAttackMoveAction.ExecuteElementalMove(usersInfo: unitDataSceneData.AllyUnitData, targetsInfo: unitDataSceneData.EnemyUnitData);


            /*  Before we process it, we need to determine the Targets of the attack. The ElementalMoveAction will dictate who it targets.  */
            



            //CombatAttackHandler.ProcessAttackStep(this, info, users.FirstOrDefault().GetUnitIntentData());

        }


        private IElementalMoveAction GetElementalCombination(Element elementValueA, Element elementValueB)
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