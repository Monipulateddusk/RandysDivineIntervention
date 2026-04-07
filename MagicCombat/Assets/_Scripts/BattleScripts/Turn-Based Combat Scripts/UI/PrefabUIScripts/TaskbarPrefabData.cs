using UnityEngine;
using UnityEngine.Assertions;


namespace TurnBased.UI
{
    public class TaskbarPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        [SerializeField] private RectTransform OS_StartButtonPivotTransform;
        [SerializeField] private RectTransform WindowGridTransform;
        [SerializeField] private RectTransform TaskbarHomeBoxPivotTransform;


        public RectTransform GetOS_StartButtonPivotTransform()
        {
            Assert.IsNotNull(OS_StartButtonPivotTransform);
            return OS_StartButtonPivotTransform;
        }

        public RectTransform GetWindowGridTransform()
        {
            Assert.IsNotNull(WindowGridTransform);
            return WindowGridTransform;
        }

        public RectTransform GetTaskbarHomeBoxPivotTransform()
        {
            Assert.IsNotNull(TaskbarHomeBoxPivotTransform);
            return TaskbarHomeBoxPivotTransform;
        }
    }
}