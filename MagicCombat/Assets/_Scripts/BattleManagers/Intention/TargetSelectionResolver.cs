namespace TurnBased.Intention
{
    public static class TargetSelectionResolver
    {
        public static event System.Action<UnitIndex> OnTargetSelectionComplete;
        public static void ProcessIntentionTargetSelection(UnitIndex unitIndex)
        {

            OnTargetSelectionComplete?.Invoke(unitIndex);
        }
    }
}