using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoSingleton<Fade>
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Image panel;

    private CancellationTokenSource cts;

    protected override void Awake()
    {
        base.Awake();
        canvas.enabled = false;
    }

    public async UniTask FadeInAsync(float duration = 0.5f)
    {
        cts?.Cancel();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        try
        {
            canvas.enabled = true;
            panel.color = Color.black;
            await panel.DOFade(0f, duration).ToUniTask(cancellationToken: cts.Token);
            canvas.enabled = false;
        }
        finally
        {
            cts = null;
        }
    }

    public async UniTask FadeOutAsync(float duration = 0.5f)
    {
        cts?.Cancel();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        try
        {
            canvas.enabled = true;
            panel.color = Color.clear;
            await panel.DOFade(1f, duration).ToUniTask(cancellationToken: cts.Token);
            canvas.enabled = false;
        }
        finally
        {
            cts = null;
        }
    }
}
