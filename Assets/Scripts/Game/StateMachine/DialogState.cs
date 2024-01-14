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
        if (InputUtility.Up.IsTrigger()) dialog.Up();
        else if (InputUtility.Down.IsTrigger()) dialog.Down();
        if (InputUtility.Right.IsTrigger()) dialog.Right();
        else if (InputUtility.Left.IsTrigger()) dialog.Left();
        if (InputUtility.Submit.IsTrigger()) dialog.Submit();
        else if (InputUtility.Cancel.IsTrigger()) dialog.Cancel();
    }
}
