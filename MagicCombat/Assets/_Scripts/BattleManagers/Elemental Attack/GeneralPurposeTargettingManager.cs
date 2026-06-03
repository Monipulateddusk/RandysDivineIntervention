using TurnBased.Status;

namespace TurnBased
{
    public class GeneralPurposeTargettingManager
    {
        private static GeneralPurposeTargettingManager instance;
        public static GeneralPurposeTargettingManager Instance
        {
            get
            {           
                return instance;
            }
        }

        private System.Collections.Generic.Dictionary<string, TargetSelection.ITargetSelector> TargetSelectionDictionary = new();


        public void Awake(IElementalMove[,] elementalLookUpTable)
        {
            if (instance == null)
            {
                instance = this;
            }
            this.TargetSelectionDictionary = new();

            InitialiseElementalMoveTargetSelectors(elementalLookUpTable);
            InitialiseStatusTargetSelectors();
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.TargetSelectionDictionary.Clear();
            this.TargetSelectionDictionary = null;
        }

        private void InitialiseElementalMoveTargetSelectors(IElementalMove[,] elementalLookUpTable)
        {
            foreach (IElementalMove elementMoveAction in elementalLookUpTable)
            {
                if (elementMoveAction == null) { continue; }

                AddTargetSelector(elementMoveAction.GetMoveName(), elementMoveAction.GetTargetSelectorType());
            }
        }

        private void InitialiseStatusTargetSelectors()
        {
            foreach (Status.BaseStatus status in StatusReferances.StatusEffects)
            {
                AddTargetSelector(status.StatusName, status.StatusTargetType);
            }
        }

        public bool AddTargetSelector(string name, UnitTargetSelectorType targetSelectorType)
        {
            /*  If we already have this index, stop!    */
            if (this.TargetSelectionDictionary.ContainsKey(name)) { return false; }

            /*  When called, retrieve the IMoveSelector type.   */
            if (!TargetSelection.TargetSelectorHandler.TryGetSelector(targetSelectorType, out TargetSelection.ITargetSelector targetSelector)) { return false; }

            if (TargetSelection.TargetSelectorHandler.IsSelectorTypeInstanciatable(targetSelectorType))
            {
                /* Create a new instance of the class. Important for selectors like Sequential which have a unit-driven 'memory' for the previously selected move.  */
                TargetSelection.ITargetSelector newSelectorInstance = (TargetSelection.ITargetSelector)System.Activator.CreateInstance(targetSelector.GetType());

                this.TargetSelectionDictionary.Add(name, newSelectorInstance);
            }
            /*  If not, just get a referance to this Class. */
            else
            {
                this.TargetSelectionDictionary.Add(name, targetSelector);
            }

            return true;
        }

        public bool RemoveTargetSelector(string name)
        {
            /*  If we don't have this index, stop!    */
            if (!this.TargetSelectionDictionary.ContainsKey(name)) { return false; }

            this.TargetSelectionDictionary.Remove(name);
            return true;
        }

        public bool TryGetTargetSelector(string name, out TargetSelection.ITargetSelector targetSelector)
        {
            targetSelector = default;

            /*  If we don't have this index, stop!    */
            if (!this.TargetSelectionDictionary.ContainsKey(name)) { return false; }

            targetSelector = this.TargetSelectionDictionary[name];
            return true;
        }
    }
}