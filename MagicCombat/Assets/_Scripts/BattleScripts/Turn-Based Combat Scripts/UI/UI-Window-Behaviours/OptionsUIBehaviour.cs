using UnityEngine;

namespace TurnBased
{
    public class OptionsUIAttachment : DialogueBoxAttachment
    {
        public OptionsUIAttachment(SizableWindowBaseBehaviour dialogueBoxOwner) : base(dialogueBoxOwner)
        {
        }
    }

    public class OptionsUIBehaviour : MonoBehaviour
    {
        #region Volume Slider Methods
        /*  All methods here are called and attached within the Options Prefab. */
        private float GetNormalisationOfValueFromSlider(UnityEngine.UI.Slider slider)
        {
            return (slider.minValue - slider.value) / (slider.minValue - slider.maxValue);
        }

        public void OnMasterVolumeChange(UnityEngine.UI.Slider slider)
        {
            Debug.Log("Changed Master Voliume to: " + GetNormalisationOfValueFromSlider(slider));
        }
        public void OnMusicVolumeChange(UnityEngine.UI.Slider slider)
        {
            Debug.Log("Changed Music Voliume to: " + GetNormalisationOfValueFromSlider(slider));
        }
        public void OnSoundEffectsVolumeChange(UnityEngine.UI.Slider slider)
        {
            Debug.Log("Changed Sound Effect Voliume to: " + GetNormalisationOfValueFromSlider(slider));
        }

        #endregion
    }
}