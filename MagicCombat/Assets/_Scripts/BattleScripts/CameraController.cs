using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController instance;
    public static CameraController Instance
    {
        get { return instance; }
        set
        {
            if (instance == null)
            {
                instance = value;
            }
        }
    }

    UnityEngine.Rendering.Universal.UniversalAdditionalCameraData URP_CameraData;
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
    public int currentCameraIndex { get; private set; }
    private const int VERTICAL_FOV = 60, ANIMATE_DURATION = 1;
    private bool isAnimating;

    #region Initalisation
    private void Awake()
    {
        Initalise();
        SetCameraIndex(0);
    }
    private void Initalise()
    {
        InitaliseSingleton();

        Object cameraObject = FindFirstObjectByType(typeof(Camera));
        if (cameraObject == null)
        {
            CreateCameraObject();
            CreateCameraCoverObject();
        }
        else
        {
            FindCameraAndCoverInScene(cameraObject);
        }
    }

    private void InitaliseSingleton()
    {
        instance = this;
    }
    private void CreateCameraObject()
    {
        this.cameraGameObject = new GameObject("Camera", typeof(Camera), typeof(AudioListener), typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData));

        this.sceneCamera = this.cameraGameObject.GetComponent<Camera>();

        this.URP_CameraData = this.cameraGameObject.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        this.URP_CameraData.renderPostProcessing = true;
    }

    private void CreateCameraCoverObject()
    {
        /*  Create the camera cover used in camera animations.  */
        GameObject camCover = new("CameraCover", typeof(SpriteRenderer));
        this.cameraCoverSprite = camCover.GetComponent<SpriteRenderer>();

        /*  Attach the cover to the Camera GameObject as a child    */
        this.cameraCoverSprite.gameObject.transform.SetParent(this.cameraGameObject.transform);

        /*  Set the position and scale of the cover.    */
        this.cameraCoverSprite.transform.localScale = new Vector3(10, 10, 10);
        this.cameraCoverSprite.transform.localPosition = new Vector3(0, 0, 1);
    }
    private void FindCameraAndCoverInScene(Object cameraObject)
    {
        this.cameraGameObject = cameraObject.GameObject();
        this.sceneCamera = cameraObject.GetComponent<Camera>();

        if (cameraObject.GameObject().TryGetComponent(out UnityEngine.Rendering.Universal.UniversalAdditionalCameraData cameraData))
        {
            this.URP_CameraData = cameraData;
        }

        if (this.cameraGameObject.transform.GetChild(0).TryGetComponent(out SpriteRenderer sprRender))
        {
            this.cameraCoverSprite = sprRender;
        }
    }

    #endregion

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

    #region Camera Animation
    private async Task AnimateCameraFadeInOut(bool isFadingIn)
    {
        if (this.isAnimating) { return; }
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

    #endregion

    public async Task IncrementCameraIndex()
    {
        if (this.isAnimating) { return; }

        int index = this.currentCameraIndex + 1;
        if (index > this.CameraPositions.Length - 1) { index = 0; }

        await AnimateCameraFadeInOut(true);

        SetCameraIndex(index);

        await AnimateCameraFadeInOut(false);
    }
    public async Task DecrementCameraIndex()
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
        if (this.cameraGameObject == null) { return; }
        if (this.CameraPositions.Length != this.CameraRotations.Length) { return; }

        if (currentCameraIndex < (CameraPositions.Length))
        {
            this.cameraGameObject.transform.SetPositionAndRotation(this.CameraPositions[currentCameraIndex], Quaternion.Euler(this.CameraRotations[currentCameraIndex]));
        }
    }
}
