using System.Threading.Tasks;
using TurnBased;

public class TestHookExample
{
    public void Awake()
    {
        EventHookSystem.OnResolvingTurnOrderPhase += EventHookSystem_OnResolvingTurnOrder;
    }


    public void OnDestroy()
    {
        EventHookSystem.OnResolvingTurnOrderPhase -= EventHookSystem_OnResolvingTurnOrder;
    }
    private void EventHookSystem_OnResolvingTurnOrder(TurnBased.Phases.PhaseTaskCompletionManager completionManager)
    {
        completionManager.AddAction();

        _ = DelayTaskBruhWhatever(completionManager);

    }

    private async Task DelayTaskBruhWhatever(TurnBased.Phases.PhaseTaskCompletionManager completionManager)
    {
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");

        await Task.Delay(3000);

        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");
        UnityEngine.Debug.LogError("swaijdoiqjwiejqoijwiojeoiqjwieojwqoiejqwoioeiwqjoieqwjioeoqwije");


        completionManager.OnActionComplete();
    }
}
