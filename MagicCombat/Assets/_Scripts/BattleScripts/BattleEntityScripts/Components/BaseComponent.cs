using UnityEngine;

public abstract class BaseComponent : MonoBehaviour
{
    /// <summary>
    /// Static method to create and initialize a component. Utilises generic syntax to be as flexible as possible
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="gameObject"></param>
    /// <param name="initParameter"></param>
    /// <returns></returns>
    public static T CreateInstance<T, U>(GameObject gameObject, U initParameter) where T : BaseComponent
    {
        T component = gameObject.AddComponent<T>();
        component.Initialize(initParameter);
        return component;
    }

    // Abstract method to initialize the component
    protected abstract void Initialize<U>(U initParameter);
}

public class BaseCombatComponent : BaseComponent
{
    [HideInInspector] public BaseBattleUnit bBU;
    protected override void Initialize<U>(U initParameter)
    {
        if(initParameter is BaseBattleUnit unit)
        {
            Init(unit);
        }
    }

    public virtual void Init(BaseBattleUnit bBU)
    {
        this.bBU = bBU;
    }

}