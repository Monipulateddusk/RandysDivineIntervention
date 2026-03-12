using UnityEngine;

namespace TurnBased.UI
{
    public class ItemPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        public RectTransform ItemRootTransform;
        public RectTransform SliderBGTransform;
        public RectTransform BackgroundTransform;
    }
}
