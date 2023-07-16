using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogState : IState
{
    private DialogBase dialog;
    public void OnEnter(DialogBase dialog)
    {
        this.dialog = dialog;
        dialog.Open();
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
    }
}
