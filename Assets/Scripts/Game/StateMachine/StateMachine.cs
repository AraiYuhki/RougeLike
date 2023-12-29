using System;
using System.Collections.Generic;

public interface IState
{
    void OnEnter();
    void OnExit();
    void Update();
}

public abstract class StateMachine<TEnum, TState>
    where TEnum : Enum
    where TState : class, IState
{
    protected TEnum currentState = default;
    protected TEnum prevState = default;
    protected Dictionary<TEnum, TState> states = new();

    public TEnum State => currentState;

    protected TState current => states.TryGetValue(currentState, out var instance) ? instance : null;
    public void AddState(TEnum state, TState instance) => states[state] = instance;

    public virtual void Goto(TEnum state)
    {
        current?.OnExit();
        prevState = currentState;
        currentState = state;
        current?.OnEnter();
    }

    /// <summary>
    /// 一つ前のステートに戻す
    /// 現在スタックが複数ある場合に対応していないので、必要があれば対応すること
    /// </summary>
    public void GotoPrev()
    {
        current?.OnExit();
        currentState = prevState;
        current?.OnEnter();
    }
}