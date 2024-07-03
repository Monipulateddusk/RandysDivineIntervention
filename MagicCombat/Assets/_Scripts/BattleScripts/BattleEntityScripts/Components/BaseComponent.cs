using UnityEngine;

public abstract class BaseComponent : MonoBehaviour
{
    [HideInInspector] public BaseBattleUnit bBU;
    // Static method to create and initialize a component
    public static T CreateInstance<T, U>(GameObject gameObject, U initParameter) where T : BaseComponent
    {
        T component = gameObject.AddComponent<T>();
        component.Initialize(initParameter);
        return component;
    }

    // Abstract method to initialize the component
    protected abstract void Initialize<U>(U initParameter);
}

