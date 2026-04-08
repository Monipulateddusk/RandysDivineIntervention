using UnityEngine;

public class IntentionResolver
{
    private static IntentionResolver instance;

    public static IntentionResolver Instance
    {
        get 
        {
            try
            {
                return instance;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                return null;
            }
        }
    }

    public void Initalise()
    {
        /*  Initalise the Singleton.    */
        if (instance != this)
        {
            return;
        }
        instance = this;
    }


}
