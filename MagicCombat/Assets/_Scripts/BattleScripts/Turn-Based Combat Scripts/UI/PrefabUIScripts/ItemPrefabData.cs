using UnityEngine;
using UnityEngine.Assertions;

namespace TurnBased.UI
{
    public class ItemPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        [SerializeField] private RectTransform ItemRootTransform;
        [SerializeField] private RectTransform SliderBGTransform;
        [SerializeField] private RectTransform BackgroundTransform;

        public RectTransform GetItemRootTransform()
        {
            Assert.IsNotNull(ItemRootTransform);
            return ItemRootTransform;
        }


        public RectTransform GetSliderBGTransform()
        {
            Assert.IsNotNull(SliderBGTransform);
            return SliderBGTransform;
        }

        public RectTransform GetBackgroundTransform()
        {
            Assert.IsNotNull(BackgroundTransform);
            return BackgroundTransform;
        }
    }
}
