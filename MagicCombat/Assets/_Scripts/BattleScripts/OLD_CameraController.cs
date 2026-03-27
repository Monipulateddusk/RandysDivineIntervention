using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class OLD_CameraController : MonoBehaviour
{
    [Header("Inspector Variables")]
    [SerializeField] List<Transform> battlePositions = new List<Transform>();

    Quaternion originalLocalRot;
    Camera cam;
    int index = 0;

    private void Awake()
    {
        originalLocalRot = transform.rotation;
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            MoveCameraToTransform(battlePositions[index]);
            index++;
            if(index >= 6)
            {
                index = 0;
            }
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            MoveCameraBackToOrigin();
        }
 
    }
    IEnumerator MoveCameraToTarget(Quaternion rot)
    {
        const float DURATION = 1f;
        float timer = 0.0f;

        while(timer < DURATION)
        {
            float v = timer / DURATION;

            // Set the rotation over time so it is not a snap
            cam.transform.rotation = Quaternion.Lerp(transform.rotation, rot, v);


            timer += Time.deltaTime;
            yield return null;
        }
    }


    public void MoveCameraToTransform(Transform target)
    {
        // Calculate the target rot based on the transform of the camera and the target
        Vector3 relativePos = target.localPosition - transform.position;
        Quaternion rot = Quaternion.LookRotation(relativePos);

        StartCoroutine(MoveCameraToTarget(rot));
        cam.fieldOfView = 40;
    }

    public void MoveCameraBackToOrigin()
    {
        StartCoroutine(MoveCameraToTarget(originalLocalRot));

        // Reset FOV back to 60
        cam.fieldOfView = 60;
    }
}
