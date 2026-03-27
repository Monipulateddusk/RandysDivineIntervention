using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionWindowBehaviour : MonoBehaviour
{
    [Header("Inspector Variables")]
    [SerializeField, Tooltip("Supply this field with 'CameraTab' in Tabs")] UnityEngine.UI.Button CameraTabButton;
    [SerializeField, Tooltip("Supply this field with 'UnitTab' in Tabs")]   UnityEngine.UI.Button UnitTabButton;

}
