using System;
using UnityEngine;
using UnityEngine.Events;

public class UseMenuUI : ScrollMenu
{
    [SerializeField]
    private Canvas canvas;

    public override void Open(Action onComplete = null)
    {
        canvas.enabled = true;
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        base.Close(() =>
        {
            onComplete?.Invoke();
            canvas.enabled = false;
        });
    }
}
