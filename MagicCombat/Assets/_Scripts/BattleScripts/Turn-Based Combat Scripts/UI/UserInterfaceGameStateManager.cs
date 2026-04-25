using UnityEngine;

namespace TurnBased.UI
{
    public class UserInterfaceGameStateManager
    {
        private static UserInterfaceGameStateManager instance;
        public static UserInterfaceGameStateManager Instance 
        {
            get
            {
                return instance;
            }
            private set
            {
                if (instance == null)
                {
                    instance = value;
                }
            }
        }

        UnityEngine.UI.RawImage GameSceneRawImageScreen;
        private const float SCREEN_DARKEN_DURATION = 1.0f;
        private const float SCREEN_DARKEN_LOWER_CLAMP = 0.2f;


        public void Awake(UnityEngine.UI.RawImage GameSceneScreen)
        {
            instance = this;
            this.GameSceneRawImageScreen = GameSceneScreen;
        }
        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.H))
            {
                _ = DarkenScreenOverTime();
            }
        }

        public async System.Threading.Tasks.Task DarkenScreenOverTime()
        {
            Color originalScreenColour = this.GameSceneRawImageScreen.color;
            float startTime = Time.time;
            while (Time.time < startTime + SCREEN_DARKEN_DURATION)
            {
                float t = 1 - ((Time.time - startTime) / SCREEN_DARKEN_DURATION);
                float opacityValue = Mathf.Lerp(SCREEN_DARKEN_LOWER_CLAMP, 1, t);

                /*  Clamp the screen darkening between 0.2 and 1    */
                this.GameSceneRawImageScreen.color = new(originalScreenColour.r, originalScreenColour.g, originalScreenColour.b, opacityValue);

                await System.Threading.Tasks.Task.Yield();
            }

            this.GameSceneRawImageScreen.color = new(originalScreenColour.r, originalScreenColour.g, originalScreenColour.b, SCREEN_DARKEN_LOWER_CLAMP);
        } 
    }
}
