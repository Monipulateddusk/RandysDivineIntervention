using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    GameObject cameraGameObject;
    private readonly Vector3[] CameraPositions =
    {
        new (  -7,  6,  -9),
        new (   8,  6,  -3),
        new (   5,  7,   4),
        new (  -8,  9,  -3),
    };
    private readonly Vector3[] CameraRotations =
{
        new (  30,   50,  0),
        new (  40,  -90,  0),
        new (  40,  -150, 0),
        new (  45,  -270, 0),
    };
    private int currentCameraIndex;

    private void Initalise()
    {
        Object cameraObject = FindFirstObjectByType(typeof(Camera));
        if (cameraObject == null)
        {
            this.cameraGameObject = new GameObject("Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData));
            UniversalAdditionalCameraData camComp = this.cameraGameObject.GetComponent<UniversalAdditionalCameraData>();
            camComp.renderPostProcessing = true;
        }
        else
        {
            this.cameraGameObject = cameraObject.GameObject();    
        }
    }
    private void Awake()
    {
        Initalise();
        
    }

    private void Start()
    {
        currentCameraIndex = 0;
        SetCameraPosition();
    }

    void SetCameraPosition()
    {
        if(this.cameraGameObject == null) { return; }
        if(this.CameraPositions.Length != this.CameraRotations.Length) { return; }

        if (currentCameraIndex < (CameraPositions.Length -1))
        {
            this.cameraGameObject.transform.SetPositionAndRotation(this.CameraPositions[currentCameraIndex], Quaternion.Euler(this.CameraRotations[currentCameraIndex]));
        }
    }
}
