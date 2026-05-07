using UnityEngine;

namespace TurnBased.LoaderUnloader
{
    public class LevelSelectManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Assign with the Level 1 Button")] private UnityEngine.UI.Button Level1Button;
        [SerializeField, Tooltip("Assign with the Level 2 Button")] private UnityEngine.UI.Button Level2Button;
        [SerializeField, Tooltip("Assign with the Level 1 Button")] private UnityEngine.UI.Button Level3Button;
        [SerializeField, Tooltip("Assign with the ShutDown Button")] private UnityEngine.UI.Button ShutDownButton;

        [SerializeField] private System.Collections.Generic.List<LevelData> LevelData;

        [Header("Cursor Properties")]
        [SerializeField, Tooltip("REQUIRED FIELD: SLOT IN POPULATED SCRIPTABLE OBJECT!!")] private UICollection_SO UI_PrefabData;
        [SerializeField, Tooltip("Required Field. Populate with a referance to the Cursor Image Spritesheet.")] private Texture2D CursorImages;
        [SerializeField] private Transform CursorParent;
        private CursorManager CursorManager;

        private void Awake()
        {
            if (Level1Button != null || Level2Button != null || Level3Button != null || ShutDownButton != null)
            {
                this.Level1Button.onClick.AddListener(OnLevel1ButtonPressed);
                this.Level2Button.onClick.AddListener(OnLevel2ButtonPressed);
                this.Level3Button.onClick.AddListener(OnLevel3ButtonPressed);
                this.ShutDownButton.onClick.AddListener(OnShutDownButtonPressed);
            }

            if (this.UI_PrefabData == null || this.CursorImages == null || this.CursorParent == null) { return; }
            Cursor.visible = false;
            this.CursorManager = new(this.CursorParent, this.UI_PrefabData.CursorPrefab, this.CursorImages);
        }

        private void OnDestroy()
        {
            if (Level1Button != null || Level2Button != null || Level3Button != null || ShutDownButton != null)
            {
                this.Level1Button.onClick.RemoveListener(OnLevel1ButtonPressed);
                this.Level2Button.onClick.RemoveListener(OnLevel2ButtonPressed);
                this.Level3Button.onClick.RemoveListener(OnLevel3ButtonPressed);
                this.ShutDownButton.onClick.RemoveListener(OnShutDownButtonPressed);
            }

            this.CursorManager.OnDestroy();
        }

        private void Update()
        {
            this.CursorManager.Update();
        }

        private void OnLevel1ButtonPressed()
        {
            if (this.LevelData.Count > 0)
            {
                LevelLoaderManager.Instance.SwitchToMainSceneSelection(this.LevelData, 0);
            }
        }
        private void OnLevel2ButtonPressed()
        {
            if (this.LevelData.Count > 0)
            {
                LevelLoaderManager.Instance.SwitchToMainSceneSelection(this.LevelData, 1);
            }
        }
        private void OnLevel3ButtonPressed()
        {
            if (this.LevelData.Count > 0)
            {
                LevelLoaderManager.Instance.SwitchToMainSceneSelection(this.LevelData, 2);
            }
        }
        private void OnShutDownButtonPressed()
        {
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }

}