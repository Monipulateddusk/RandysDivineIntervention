namespace TurnBased.Elements
{
    public class ElementalAttackTargettingManager
    {
        private static ElementalAttackTargettingManager instance;
        public static ElementalAttackTargettingManager Instance
        {
            get
            {           
                return instance;
            }
        }

        private System.Collections.Generic.Dictionary<string, TargetSelection.ITargetSelector> ElementalMoveTargetSelectionDictionary = new();


        public void Awake(IElementalMoveAction[,] elementalLookUpTable)
        {
            if (instance == null)
            {
                instance = this;
            }
            this.ElementalMoveTargetSelectionDictionary = new();

            InitialiseMoveTargetSelectors(elementalLookUpTable);
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.ElementalMoveTargetSelectionDictionary.Clear();
            this.ElementalMoveTargetSelectionDictionary = null;
        }

        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.V))
            {
                foreach (var kvp in this.ElementalMoveTargetSelectionDictionary)
                {
                    UnityEngine.Debug.LogError($"Key: {kvp.Key} : Value is: {kvp.Value}");
                }
            }
        }

        private void InitialiseMoveTargetSelectors(IElementalMoveAction[,] elementalLookUpTable)
        {
            foreach (IElementalMoveAction elementMoveAction in elementalLookUpTable)
            {
                if (elementMoveAction == null) { continue; }

                AddElementalMoveTargetSelector(elementMoveAction.GetMoveName(), elementMoveAction.GetTargetSelectorType());
            }
        }

        public bool AddElementalMoveTargetSelector(string moveName, UnitTargetSelectorType targetSelectorType)
        {
            /*  If we already have this index, stop!    */
            if (this.ElementalMoveTargetSelectionDictionary.ContainsKey(moveName)) { return false; }

            /*  When called, retrieve the IMoveSelector type.   */
            if (!TargetSelection.TargetSelectorHandler.TryGetSelector(targetSelectorType, out TargetSelection.ITargetSelector targetSelector)) { return false; }


            if (TargetSelection.TargetSelectorHandler.IsSelectorTypeInstanciatable(targetSelectorType))
            {
                /* Create a new instance of the class. Important for selectors like Sequential which have a unit-driven 'memory' for the previously selected move.  */
                TargetSelection.ITargetSelector newSelectorInstance = (TargetSelection.ITargetSelector)System.Activator.CreateInstance(targetSelector.GetType());

                this.ElementalMoveTargetSelectionDictionary.Add(moveName, newSelectorInstance);
            }
            /*  If not, just get a referance to this Class. */
            else
            {
                this.ElementalMoveTargetSelectionDictionary.Add(moveName, targetSelector);
            }

            return true;
        }

        public bool RemoveElementalMoveTargetSelector(string moveName)
        {
            /*  If we don't have this index, stop!    */
            if (!this.ElementalMoveTargetSelectionDictionary.ContainsKey(moveName)) { return false; }

            this.ElementalMoveTargetSelectionDictionary.Remove(moveName);
            return true;
        }

        public bool TryGetTargetSelector(string moveName, out TargetSelection.ITargetSelector targetSelector)
        {
            targetSelector = default;

            /*  If we don't have this index, stop!    */
            if (!this.ElementalMoveTargetSelectionDictionary.ContainsKey(moveName)) { return false; }

            targetSelector = this.ElementalMoveTargetSelectionDictionary[moveName];
            return true;
        }
    }
}