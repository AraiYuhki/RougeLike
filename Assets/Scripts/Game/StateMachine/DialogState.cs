using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogState : IState
{
    private DialogBase dialog;
    public void OnEnter(DialogBase dialog)
    {
        this.dialog = dialog;
    }
    public void OnEnter()
    {
    }

    public void OnExit()
    {
        DialogManager.Instance.Close(dialog);
        dialog = null;
    }

    public void Update()
    {
        if (InputUtility.Up.IsTriggerd()) dialog.Up();
        else if (InputUtility.Down.IsTriggerd()) dialog.Down();
        if (InputUtility.Right.IsTriggerd()) dialog.Right();
        else if (InputUtility.Left.IsTriggerd()) dialog.Left();
        if (InputUtility.Submit.IsTriggerd()) dialog.Submit();
        else if (InputUtility.Cancel.IsTriggerd()) dialog.Cancel();
    }
}
