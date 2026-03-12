using UnityEngine;

namespace TurnBased.UI
{
    public class SlotPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        public RectTransform SlotRootTransform;
        public RectTransform UnitImageTransform;
    }

}