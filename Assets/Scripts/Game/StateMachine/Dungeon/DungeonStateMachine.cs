using System;
using System.Collections;

public enum GameState
{
    Wait,
    PlayerTurn,
    EnemyTurn,
    UIControll,
    TurnEnd,
    MainMenu,
    NextFloorLoad,
    Dialog,
    Shop,
}

public class DungeonStateMachine : StateMachine<GameState, IState>, IDungeonStateMachine
{
    public void OpenCommonDialog(string title, string message, params (string label, Action onClick)[] data)
    {
        prevState = currentState;
        var dialog = DialogManager.Instance.Create<CommonDialog>();
        dialog.Initialize(title, message, data);
        dialog.Open(() =>
        {
            currentState = GameState.Dialog;
            if (current is DialogState dialogState)
                dialogState?.OnEnter(dialog);
            else
                throw new Exception("Can't enter dialog state");
        });
        currentState = GameState.Wait;
    }

    public IEnumerator Update()
    {
        while (true)
        {
            if (current != null)
            {
                current.Update();
            }
            yield return null;
        }
    }
}
