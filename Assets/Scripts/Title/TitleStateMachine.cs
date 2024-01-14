using System;

public enum TitleState
{
    Wait,
    MainMenu,
    Config,
    Dialog,
}

public class TitleStateMachine : StateMachine<TitleState, IState>
{
    public void OpenCommonDialog(string title, string message, params (string label, Action onClick)[] data)
    {
        prevState = currentState;
        var dialog = DialogManager.Instance.Create<CommonDialog>();
        dialog.Initialize(title, message, data);
        dialog.Open(() =>
        {
            currentState = TitleState.Dialog;
            if (current is DialogState dialogState)
                dialogState?.OnEnter(dialog);
            else
                throw new Exception("Can't enter dialog state");
        });
        currentState = TitleState.Wait;
    }

    public void Update()
    {
        current?.Update();
    }
}
