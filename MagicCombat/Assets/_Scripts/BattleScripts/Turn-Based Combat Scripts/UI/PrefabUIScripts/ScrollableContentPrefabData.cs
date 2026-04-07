using UnityEngine;
using UnityEngine.Assertions;

namespace TurnBased.UI
{
    public class ScrollableContentPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        [SerializeField] private RectTransform ScrollableContentBGTransform;
        [SerializeField] private RectTransform ScrollableRootTransform;
        [SerializeField] private RectTransform ScrollableViewportTransform;
        [SerializeField] private RectTransform ScrollableContentTransform;


        public RectTransform GetScrollableContentBGTransform()
        {
            Assert.IsNotNull(ScrollableContentBGTransform);
            return ScrollableContentBGTransform;
        }


        public RectTransform GetScrollableRootTransform()
        {
            Assert.IsNotNull(ScrollableRootTransform);
            return ScrollableRootTransform;
        }


        public RectTransform GetScrollableViewportTransform()
        {
            Assert.IsNotNull(ScrollableViewportTransform);
            return ScrollableViewportTransform;
        }


        public RectTransform GetScrollableContentTransform()
        {
            Assert.IsNotNull(ScrollableContentTransform);
            return ScrollableContentTransform;
        }
    }
}