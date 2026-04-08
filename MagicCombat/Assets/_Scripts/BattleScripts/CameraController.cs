using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

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
    UnityEngine.Rendering.Volume cameraLocalisedVolume;
    UnityEngine.Rendering.Universal.UniversalAdditionalCameraData URP_CameraData;
    Camera sceneCamera;
    [SerializeField]GameObject cameraGameObject, blendCameraGameObject;
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
    public int CurrentCameraIndex { get; private set; }
    private const int VERTICAL_FOV = 60, SCREEN_SHAKE_INTENSITY = 2;
    [SerializeField] RawImage GameScreen;
    [SerializeField] VolumeProfile volumeProfile;
    [SerializeField] Material shaderMaterial;

    [SerializeField, Range(0, 100)] float horizontalShake, VerticalShake; 

    [SerializeField, Range(0.01f, 1)] private float ANIMATE_DURATION = 0.025f;
    [SerializeField] Sprite[] cameraStaticSprites;
    [SerializeField] Color cameraStaticColor;
    [SerializeField] CameraAnimType isFadingInAndOut = CameraAnimType.Fade;
    private bool isAnimating;

    #region Shader
    ChromaticAberration volumeChromaticAberration;
    DepthOfField volumeDepthOfField;

    private void InitialiseShader()
    {
        if (this.volumeProfile == null) { return; }
        
        if(this.volumeProfile.TryGet(out ChromaticAberration chromaticAberration))
        {
            this.volumeChromaticAberration = chromaticAberration;
        }
        if(this.volumeProfile.TryGet(out DepthOfField depthOfField))
        {
            this.volumeDepthOfField = depthOfField;
        }

        this.shaderMaterial = this.GameScreen.material;
    }

    #endregion

    #region Initalisation
    private void Awake()
    {
        Initalise();
        InitialiseShader();
        SetCameraIndex(0);
    }
    private void Initalise()
    {
        InitaliseSingleton();

        UnityEngine.Object cameraObject = FindFirstObjectByType(typeof(Camera));
        if (cameraObject == null)
        {
            CreateCameraObject();
            CreateCameraCoverObject();
        }
        else
        {
            FindCameraInfoInScene(cameraObject);
        }
    }

    private void InitaliseSingleton()
    {
        instance = this;
    }
    private void CreateCameraObject()
    {
        this.cameraGameObject = new GameObject("Camera", typeof(Camera), typeof(AudioListener), typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData), typeof(UnityEngine.Rendering.Volume));

        this.sceneCamera = this.cameraGameObject.GetComponent<Camera>();

        this.URP_CameraData = this.cameraGameObject.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        this.cameraLocalisedVolume = this.cameraGameObject.GetComponent<UnityEngine.Rendering.Volume>();
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
    private void FindCameraInfoInScene(UnityEngine.Object cameraObject)
    {
        this.cameraGameObject = cameraObject.GameObject();
        this.sceneCamera = cameraObject.GetComponent<Camera>();

        // Get the URP Data
        if (cameraObject.GameObject().TryGetComponent(out UnityEngine.Rendering.Universal.UniversalAdditionalCameraData cameraData))
        {
            this.URP_CameraData = cameraData;
        }

        // Get the sprite renderer for the cover
        if (this.cameraGameObject.transform.GetChild(0).TryGetComponent(out SpriteRenderer sprRender))
        {
            this.cameraCoverSprite = sprRender;
        }
        
        // Get the localised Rendering volume for PostProcessing
        if(this.cameraGameObject.TryGetComponent(out UnityEngine.Rendering.Volume volume)){
            this.cameraLocalisedVolume = volume;
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
       // this.cameraCoverSprite.color = new(0f, 0f, 0f, colorAlphaEnd);
        this.sceneCamera.fieldOfView = fieldOfViewEnd;
        return;
    }

    private async Task AnimateCameraMoveToNextPosition(Vector3 currentPosition, Vector3 nextPosition, Vector3 currentRotation, Vector3 nextRotation)
    {
        if (this.isAnimating) { return; }
        this.isAnimating = true;

        float startTime = Time.time;
        while (Time.time < startTime + ANIMATE_DURATION)
        {
            float t = (Time.time - startTime) / ANIMATE_DURATION;

            /*  Next incremental rotation and position values.  */
            Vector3 pos = new(
                Mathf.Lerp(currentPosition.x, nextPosition.x, t), 
                Mathf.Lerp(currentPosition.y, nextPosition.y, t), 
                Mathf.Lerp(currentPosition.z, nextPosition.z, t)
                            );
            Vector3 rot = new(
                Mathf.Lerp(currentRotation.x, nextRotation.x, t),
                Mathf.Lerp(currentRotation.y, nextRotation.y, t),
                Mathf.Lerp(currentRotation.z, nextRotation.z, t)
                            );

            this.cameraGameObject.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

            await Task.Yield();
        }

        /*  Once we are done animating, set the values in the event of any floating points. */
        this.isAnimating = false;
        this.cameraGameObject.transform.SetPositionAndRotation(nextPosition, Quaternion.Euler(nextRotation));
        return;
    }

    private async Task AnimateCameraStatic()
    {
        if(this.cameraStaticSprites == null || this.cameraStaticSprites.Length == 0) { return; }
        if (this.isAnimating) { return; }
        this.isAnimating = true;
        this.cameraCoverSprite.color = cameraStaticColor;

        float startTime = Time.time;
        while (Time.time < startTime + ANIMATE_DURATION)
        {
            float t = (Time.time - startTime) / ANIMATE_DURATION;

            int frame = Mathf.Min(Mathf.FloorToInt(t / 0.15f), this.cameraStaticSprites.Length - 1);
            this.cameraCoverSprite.sprite = this.cameraStaticSprites[frame];

            await Task.Yield();
        }

        /*  Once we are done animating, set the values in the event of any floating points. */
        this.isAnimating = false;
        this.cameraCoverSprite.color = new(0,0,0,0);

        return;
    }
    private async Task AnimateCameraShader(int nextIndex, bool isMovingLeft)
    {
        if (this.isAnimating) { return; }
        this.isAnimating = true;

        /*  Enable the postprocessing effects and reset them.   */
        this.volumeChromaticAberration.active = true;
        this.volumeChromaticAberration.intensity.Override(0);
        this.volumeDepthOfField.active = true;
        this.volumeDepthOfField.focalLength.Override(0);

        /*  Set the main camera to this position, set the blend camera to the next position.    */
        SetGameObjectPosition(this.cameraGameObject, this.CameraPositions[this.CurrentCameraIndex], this.CameraRotations[this.CurrentCameraIndex]);
        SetGameObjectPosition(this.blendCameraGameObject, this.CameraPositions[nextIndex], this.CameraRotations[nextIndex]);

        float startTime = Time.time;
        while (Time.time < startTime + ANIMATE_DURATION)
        {
            float t = (Time.time - startTime) / ANIMATE_DURATION;
            float shaderNormalisation = (t - 0) / (1 - 0);


            int frame = Mathf.Min(Mathf.FloorToInt(t / 0.1f), 10);

            /*  Set the Blend shader's blend value. */
            this.shaderMaterial.SetFloat("_BlendModifier", shaderNormalisation);

            // over the course of start-0.3, add the chromatic aberration and depth of field fade in
            if (frame < 3)
            {
                float normalisation = (t - 0) / (0.3f - 0); 

                this.volumeChromaticAberration.intensity.Override(normalisation);
                this.volumeDepthOfField.focalLength.Override(180.0f / normalisation);

                await Task.Yield();
            }
            // over the course of 0.3-0.6, Stretch the screen to the Right / Left depending on if we are incrementing or decrementing
            else if (frame < 6)
            {
                // Pull to the right
                if (isMovingLeft)
                {
                    this.GameScreen.rectTransform.SetRight(Mathf.Lerp(0, -1000, Time.deltaTime));
                    this.GameScreen.rectTransform.SetLeft(Mathf.Lerp(0, -2000, Time.deltaTime));
                }
                else
                {
                    this.GameScreen.rectTransform.SetRight(Mathf.Lerp(0, -2000, Time.deltaTime));
                    this.GameScreen.rectTransform.SetLeft(Mathf.Lerp(0, -1000, Time.deltaTime));
                }

                // Screen shake loosely on the Y
                this.GameScreen.rectTransform.anchoredPosition = new()
                {
                    x = this.GameScreen.rectTransform.anchoredPosition.x,
                    y = Mathf.Cos(Time.time * VerticalShake) * SCREEN_SHAKE_INTENSITY
                };
                await Task.Yield();
            }
            else if(frame < 8)
            {
                // Pull to the right
                if (isMovingLeft)
                {
                    this.GameScreen.rectTransform.SetRight(Mathf.Lerp(-1000, -1400, Time.deltaTime));
                    this.GameScreen.rectTransform.SetLeft(Mathf.Lerp(-2000, -200, Time.deltaTime));
                }
                else
                {
                    this.GameScreen.rectTransform.SetRight(Mathf.Lerp(-2000, -200, Time.deltaTime));
                    this.GameScreen.rectTransform.SetLeft(Mathf.Lerp(-1000, -1400, Time.deltaTime));
                }

                // Screen shake loosely on the Y
                this.GameScreen.rectTransform.anchoredPosition = new()
                {
                    x = this.GameScreen.rectTransform.anchoredPosition.x,
                    y = Mathf.Cos(Time.time * VerticalShake) * SCREEN_SHAKE_INTENSITY
                };
                await Task.Yield();
            }

            // over the course of 0.6-end, add the chromatic aberration and depth of field fade out
            else
            {
                if (isMovingLeft)
                {
                    this.GameScreen.rectTransform.SetRight(Mathf.Lerp(-1400, 0, Time.deltaTime));
                    this.GameScreen.rectTransform.SetLeft(Mathf.Lerp(-200, 0, Time.deltaTime));
                }
                else
                {
                    this.GameScreen.rectTransform.SetRight(Mathf.Lerp(-200, 0, Time.deltaTime));
                    this.GameScreen.rectTransform.SetLeft(Mathf.Lerp(-1400, 0, Time.deltaTime));
                }
     


                float normalisation = (t - 0.3f) / (0.6f - 0.3f);

                this.volumeChromaticAberration.intensity.Override(normalisation);
                this.volumeDepthOfField.focalLength.Override(180.0f / normalisation);

                await Task.Yield();
            }
        }

        /*  Once we are done animating, set the values in the event of any floating points. */
        this.isAnimating = false;

        this.volumeChromaticAberration.active = false;
        this.volumeDepthOfField.active = false;

        /*  Reset the screen shake to defaults. */
        this.GameScreen.rectTransform.anchoredPosition = Vector2.zero;

        this.GameScreen.rectTransform.SetRight(0);
        this.GameScreen.rectTransform.SetLeft(0);


        /*  Set main camera to the next camera position and reset the blend.    */
        SetCameraIndex(nextIndex);
        this.shaderMaterial.SetFloat("_BlendModifier", 0);

        return;
    }
    #endregion

    public async Task IncrementCameraIndex()
    {
        if (this.isAnimating) { return; }

        int index = this.CurrentCameraIndex + 1;
        if (index > this.CameraPositions.Length - 1) { index = 0; }

        if (isFadingInAndOut == CameraAnimType.Fade)
        {
            await AnimateCameraFadeInOut(true);

            SetCameraIndex(index);

            await AnimateCameraFadeInOut(false);
        }
        else if(isFadingInAndOut == CameraAnimType.Shift)
        {
            Vector3 curPos  = this.CameraPositions[this.CurrentCameraIndex];
            Vector3 nextPos = this.CameraPositions[index];
            Vector3 curRot  = this.CameraRotations[this.CurrentCameraIndex];
            Vector3 nextRot = this.CameraRotations[index];

            await AnimateCameraMoveToNextPosition(curPos, nextPos, curRot, nextRot);
            SetCameraIndex(index);
        }
        else if (isFadingInAndOut == CameraAnimType.Static)
        {
            SetCameraIndex(index);
            await AnimateCameraStatic();
        }
        else if (isFadingInAndOut == CameraAnimType.Shader)
        {
            await AnimateCameraShader(index, false);
        }
    }
    public async Task DecrementCameraIndex()
    {
        if (this.isAnimating) { return; }

        int index = this.CurrentCameraIndex - 1;
        if (index < 0) { index = this.CameraPositions.Length - 1; }

        if (isFadingInAndOut == CameraAnimType.Fade)
        {
            await AnimateCameraFadeInOut(true);

            SetCameraIndex(index);

            await AnimateCameraFadeInOut(false);
        }
        else if (isFadingInAndOut == CameraAnimType.Shift)
        {
            Vector3 curPos = this.CameraPositions[this.CurrentCameraIndex];
            Vector3 nextPos = this.CameraPositions[index];
            Vector3 curRot = this.CameraRotations[this.CurrentCameraIndex];
            Vector3 nextRot = this.CameraRotations[index];

            await AnimateCameraMoveToNextPosition(curPos, nextPos, curRot, nextRot);
            SetCameraIndex(index);
        }
        else if (isFadingInAndOut == CameraAnimType.Static)
        {
            SetCameraIndex(index);
            await AnimateCameraStatic();            
        }
        else if(isFadingInAndOut == CameraAnimType.Shader)
        {
            await AnimateCameraShader(index, true);
        }
    }

    private void SetCameraIndex(int index)
    {
        if (CurrentCameraIndex == index) return;
        if (index > this.CameraPositions.Length - 1) { return; }

        this.CurrentCameraIndex = index;
        SetCameraPosition();
    }

    void SetCameraPosition()
    {
        if (this.cameraGameObject == null) { return; }
        if (this.CameraPositions.Length != this.CameraRotations.Length) { return; }

        if (CurrentCameraIndex < (CameraPositions.Length))
        {
            this.cameraGameObject.transform.SetPositionAndRotation(this.CameraPositions[CurrentCameraIndex], Quaternion.Euler(this.CameraRotations[CurrentCameraIndex]));
        }
    }

    void SetGameObjectPosition(GameObject gameObject, Vector3 position, Vector3 rotation)
    {
        if (gameObject == null) { return; }

        gameObject.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
    }
}
