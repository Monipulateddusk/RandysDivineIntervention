using UnityEngine;
using UnityEngine.Assertions;

namespace TurnBased.UI
{
    public class SlotPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        [SerializeField] private RectTransform SlotRootTransform;
        [SerializeField] private RectTransform UnitImageTransform;

        public RectTransform GetSlotRootTransform()
        {
            Assert.IsNotNull(SlotRootTransform);
            return SlotRootTransform;
        }

        public RectTransform GetUnitImageTransform()
        {
            Assert.IsNotNull(UnitImageTransform);
            return UnitImageTransform;
        }

    }

}