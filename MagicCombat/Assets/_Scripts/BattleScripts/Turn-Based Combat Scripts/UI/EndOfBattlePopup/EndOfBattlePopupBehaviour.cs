using TurnBased.LoaderUnloader;
using UnityEngine;

public class EndOfBattlePopupBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip("Assign with the TMP element in 'AlertText'")]         private TMPro.TextMeshProUGUI AlertTextTmp;
    [SerializeField, Tooltip("Assign with the TMP element in 'DialogueText'")]      private TMPro.TextMeshProUGUI DialogueTextTmp;
    [SerializeField, Tooltip("Assign with the Button component from 'NoButton'")]   private UnityEngine.UI.Button NoButton;
    [SerializeField, Tooltip("Assign with the Button component from 'YesButton'")]  private UnityEngine.UI.Button YesButton;
    [SerializeField, Tooltip("Assign with the CanvasGroup component on this Root Object")] private CanvasGroup CanvasGroup;

    private void Awake()
    {
        if (TurnBased.GameState.GameStateManager.Instance != null)
        {
            Initalise(TurnBased.GameState.GameStateManager.Instance.CurrentGameState);
        }
        else
        {
            Initalise(MetaGameState.Running);
        }
    }

    private void OnDestroy()
    {
        this.NoButton.onClick.RemoveAllListeners();
        this.YesButton.onClick.RemoveAllListeners();    
    }

    public void Initalise(MetaGameState metaGameState)
    {
        switch (metaGameState)
        {
            default:
            case MetaGameState.Running:

                VisualiseRunningPopup();
                break;
            case MetaGameState.PlayerWin:

                VisualisePlayerWinPopup();
                break;
            case MetaGameState.PlayerLoss:

                VisualisePlayerLossPopup();
                break;

        }
    }

    private void VisualiseRunningPopup()
    {
        this.CanvasGroup.alpha = 0;
        this.AlertTextTmp.text = string.Empty;
        this.DialogueTextTmp.text = string.Empty;
    }

    private void VisualisePlayerWinPopup()
    {
        this.CanvasGroup.alpha = 1;
        this.AlertTextTmp.text = "No Enemy Combatants Detected.\r\n";
        this.DialogueTextTmp.text = "Would You like To Move To The Next Room?";

        this.NoButton.onClick.AddListener(LevelLoaderManager.Instance.SwitchToLevelSelection);
        this.YesButton.onClick.AddListener(LevelLoaderManager.Instance.LoadNextLevel);
    }
    private void VisualisePlayerLossPopup()
    {
        this.CanvasGroup.alpha = 1;
        this.AlertTextTmp.text = "All Allied Combatants Missing.\r\n";
        this.DialogueTextTmp.text = "Would you like to try again?";

        this.NoButton.onClick.AddListener(LevelLoaderManager.Instance.SwitchToLevelSelection);
        this.YesButton.onClick.AddListener(LevelLoaderManager.Instance.ReloadCurrentScene);
    }
}
