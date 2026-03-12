using UnityEngine;

namespace TurnBased.UI
{
    public class ScrollableContentPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        public RectTransform ScrollableContentBGTransform;
        public RectTransform ScrollableRootTransform;
        public RectTransform ScrollableViewportTransform;
        public RectTransform ScrollableContentTransform;
    }
}