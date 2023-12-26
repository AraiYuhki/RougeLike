using System;

public interface IStateMachine<T> where T : Enum
{
    void Goto(T state);
}

public interface IOpenCommonDialog
{
    void OpenCommonDialog(string title, string message, params (string label, Action onClick)[] data);
}

public interface IDungeonStateMachine : IStateMachine<GameState>, IOpenCommonDialog { }

public interface IState
{
    void OnEnter();
    void OnExit();
    void Update();
}