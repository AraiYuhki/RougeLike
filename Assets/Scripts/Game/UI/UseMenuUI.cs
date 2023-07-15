using UnityEngine;
using UnityEngine.Events;

public class UseMenuUI : ScrollMenu
{
    [SerializeField]
    private Canvas canvas;

    public override void Open(UnityAction onComplete = null)
    {
        canvas.enabled = true;
        base.Open(onComplete);
    }

    public override void Close(UnityAction onComplete = null)
    {
        base.Close(() =>
        {
            onComplete?.Invoke();
            canvas.enabled = false;
        });
    }
}
