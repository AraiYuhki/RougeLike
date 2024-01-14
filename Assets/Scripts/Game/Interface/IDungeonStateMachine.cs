using System;

public interface IDungeonStateMachine
{
    public void OpenCommonDialog(string titile, string message, params (string label, Action onClick)[] data);
    public void Goto(GameState state);
}
