using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    UniversalAdditionalCameraData URP_CameraData;
    Camera sceneCamera;
    GameObject cameraGameObject;
    SpriteRenderer cameraCoverSprite;
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
    private const int VERTICAL_FOV = 60, ANIMATE_DURATION = 1;
    private bool isAnimating;

    private void Initalise()
    {
        Object cameraObject = FindFirstObjectByType(typeof(Camera));
        if (cameraObject == null)
        {
            this.cameraGameObject = new GameObject("Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData));

            this.sceneCamera = this.cameraGameObject.GetComponent<Camera>();

            this.URP_CameraData = this.cameraGameObject.GetComponent<UniversalAdditionalCameraData>();
            this.URP_CameraData.renderPostProcessing = true;

            /*  Create the camera cover used in camera animations.  */
            GameObject camCover = new("CameraCover", typeof(SpriteRenderer));
            this.cameraCoverSprite = camCover.GetComponent<SpriteRenderer>();  
            
            /*  Attach the cover to the Camera GameObject as a child    */
            this.cameraCoverSprite.gameObject.transform.SetParent(this.cameraGameObject.transform);

            /*  Set the position and scale of the cover.    */
            this.cameraCoverSprite.transform.localScale = new Vector3(10, 10, 10);
            this.cameraCoverSprite.transform.localPosition = new Vector3(0, 0, 1);
        }
        else
        {
            this.cameraGameObject = cameraObject.GameObject();
            this.sceneCamera = cameraObject.GetComponent<Camera>();

            if(cameraObject.GameObject().TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                this.URP_CameraData = cameraData;
            }

            if (this.cameraGameObject.transform.GetChild(0).TryGetComponent(out SpriteRenderer sprRender))
            {
                this.cameraCoverSprite = sprRender;
            }
        }
    }
    private void Awake()
    {
        Initalise();
        
    }

    private void Start()
    {
        SetCameraIndex(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
           _ = IncrementCameraIndex();
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
           _ = DecrementCameraIndex();
        }
    }

    private async Task AnimateCameraFadeInOut(bool isFadingIn)
    {
        if(this.isAnimating) { return; }    
        this.isAnimating = true;

        /*  Set local variables based on if we are fading out.    */
        float colorAlphaStart = 1, colorAlphaEnd = 0;
        float fieldOfViewStart = 0, fieldOfViewEnd = VERTICAL_FOV;

        /*  Re assign the values if we are fading in.   */
        if (isFadingIn)
        {
            colorAlphaStart = 0;
            colorAlphaEnd = 1;
            fieldOfViewStart = VERTICAL_FOV;
            fieldOfViewEnd = 0;
        }

        float startTime = Time.time;
        while (Time.time < startTime + ANIMATE_DURATION)
        {
            float t = (Time.time - startTime) / ANIMATE_DURATION;

            /*  Visual flare of the Camera. We want it scale the FOV in or out depending on if we are fading in or out. */
            this.sceneCamera.fieldOfView = Mathf.Lerp(fieldOfViewStart, fieldOfViewEnd, t);

            /*  Additionally, we increase or decrease the transparency of the Cover if we are fading in or out. */
            this.cameraCoverSprite.color = new(0f, 0f, 0f, Mathf.Lerp(colorAlphaStart, colorAlphaEnd, t));

            await Task.Yield();
        }

        /*  Once we are done animating, set the values in the event of any floating points. */
        this.isAnimating = false;
        this.cameraCoverSprite.color = new(0f, 0f, 0f, colorAlphaEnd);
        this.sceneCamera.fieldOfView = fieldOfViewEnd;
        return;
    }


    private async Task IncrementCameraIndex()
    {
        if (this.isAnimating) { return; }

        int index = this.currentCameraIndex + 1;
        if(index > this.CameraPositions.Length - 1) { index = 0; }

        await AnimateCameraFadeInOut(true);

        SetCameraIndex(index);

        await AnimateCameraFadeInOut(false);
    }
    private async Task DecrementCameraIndex()
    {
        if (this.isAnimating) { return; }

        int index = this.currentCameraIndex - 1;
        if (index < 0) { index = this.CameraPositions.Length - 1; }

        await AnimateCameraFadeInOut(true);

        SetCameraIndex(index);

        await AnimateCameraFadeInOut(false);
    }


    private void SetCameraIndex(int index)
    {
        if (currentCameraIndex == index) return;
        if (index > this.CameraPositions.Length - 1) { return; }
        
        currentCameraIndex = index;
        SetCameraPosition();
    }

    void SetCameraPosition()
    {
        if(this.cameraGameObject == null) { return; }
        if(this.CameraPositions.Length != this.CameraRotations.Length) { return; }

        if (currentCameraIndex < (CameraPositions.Length))
        {
            this.cameraGameObject.transform.SetPositionAndRotation(this.CameraPositions[currentCameraIndex], Quaternion.Euler(this.CameraRotations[currentCameraIndex]));
        }
    }
}
