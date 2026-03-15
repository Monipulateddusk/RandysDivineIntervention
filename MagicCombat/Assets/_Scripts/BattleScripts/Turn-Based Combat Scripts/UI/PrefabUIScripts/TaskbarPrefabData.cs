using UnityEngine;
using UnityEngine.Assertions;


namespace TurnBased.UI
{
    public class TaskbarPrefabData : MonoBehaviour
    {
        [Header("Serialised Variables. Assign in Inspector so we aren't using 'Transform.Find()'!")]
        [SerializeField] private RectTransform OS_StartButtonTransform;
        [SerializeField] private RectTransform WindowGridTransform;
        [SerializeField] private RectTransform TaskbarHomeBoxTransform;


        public RectTransform GetOSStartButtonTransform()
        {
            Assert.IsNotNull(OS_StartButtonTransform);
            return OS_StartButtonTransform;
        }

        public RectTransform GetWindowGridTransform()
        {
            Assert.IsNotNull(WindowGridTransform);
            return WindowGridTransform;
        }

        public RectTransform GetTaskbarHomeBoxTransform()
        {
            Assert.IsNotNull(TaskbarHomeBoxTransform);
            return TaskbarHomeBoxTransform;
        }
    }
}