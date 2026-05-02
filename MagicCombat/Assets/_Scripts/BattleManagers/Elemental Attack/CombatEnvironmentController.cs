using System.Linq;

namespace TurnBased.AttackResolution
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
        /*  Elemental Look-Up Table     */
        /*  Same size as the Element Enum. Units are able to imbue the Environment with their Element to do an attack if their Ally participates.   */
        readonly IElementalMoveAction[,] ElementalMoveLookUpTable = new IElementalMoveAction[7, 7]
        {   /*  NULL,   Fire                Water                   Ice                     Earth                       Light                   Darkness    */
        {   null,   null,               null,                   null,                   null,                       null,                   null,  },   /* NULL     */
        {   null,   new EM_Inferno(),   new EM_Steam(),         new EM_Frostburn(),     new EM_Volcano(),           null,                   null,  },   /* Fire     */
        {   null,   new EM_Steam(),     new EM_Tsunami(),       new EM_HailCloak(),     new EM_Wellspring(),        null,                   null,  },   /* Water    */
        {   null,   new EM_Frostburn(), new EM_HailCloak(),     new EM_IceAge(),        new EM_FrostLock(),         null,                   null,  },   /* Ice      */
        {   null,   new EM_Volcano(),   new EM_Wellspring(),    new EM_FrostLock(),     new EM_Fissure(),           null,                   null,  },   /* Earth    */
        {   null,   null,               null,                   null,                   null,                       null,                   null,  },   /* Light    */
        {   null,   null,               null,                   null,                   null,                       null,                   null,  }    /* Darkness */
        };

        private readonly System.Collections.Generic.List<ImbuedEnvironmentElement> EnvironmentEffects;

        public CombatEnvironmentController()
        {
            EnvironmentEffects = new();
        }

        public void AddEnvironmentalEffect(Element effect, UnitTeam team)
        {
            /*  Add the imbued element to the list. */
            this.EnvironmentEffects.Add(new ImbuedEnvironmentElement(team, effect));

            /*  Check the List. Does it contain two entries on the same team? If so, process that corresponding elemental attack.   */
            System.Collections.Generic.Queue<ImbuedEnvironmentElement> pairedElementsForElementalAttack = new(this.EnvironmentEffects.Where(iEE => iEE.team == team));
            if (pairedElementsForElementalAttack.Count >= 2)
            {
                for (int i = 0; i < pairedElementsForElementalAttack.Count; i++)
                {
                    EnvironmentEffects.Remove(pairedElementsForElementalAttack.ToList()[i]);
                }


                /*  Get the two elements to process it. */
                Element elementA = pairedElementsForElementalAttack.Dequeue().imbuedEnvironmentElement;
                Element elementB = pairedElementsForElementalAttack.Dequeue().imbuedEnvironmentElement;
                ProcessElementalMove(elementA, elementB, team);
            }
        }


        public void ProcessElementalMove(Element elementValueA, Element elementValueB, UnitTeam team)
        {
            // Find out what move the elements combine into and do that move to get the info needed to resolve it
            IElementalMoveAction elementalAttackMoveAction = GetElementalCombination(elementValueA, elementValueB);
            if (elementalAttackMoveAction == null) { UnityEngine.Debug.LogError("ELEMENTAL COMBINATION ERROR: NOT VALID!!!"); return; }

            /*  Get the first entry of Units on the Team to retrieve the attack information.    */
            System.Collections.Generic.List<UnitIndex> unitsOnTeam = StationManager.Instance.GetUnitsOnTeam(team);
            if (unitsOnTeam.Count <= 0) { return; }

            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(unitsOnTeam.FirstOrDefault(), out UnitData_SceneData_UnitTurn unitDataSceneData)) { return; }

            /*  Process the attack using the Scene Unit Data.   */
            AttackResolutionInfo elementalAttackResolutionInfo = elementalAttackMoveAction.DoElementalMove(usersInfo: unitDataSceneData.AllyUnitData, targetsInfo: unitDataSceneData.EnemyUnitData);


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