using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoSingleton<Fade>
{
    [SerializeField]
    private Image panel;

    private Tween tween = null;

    protected override void Awake()
    {
        base.Awake();
        panel.gameObject.SetActive(false);
    }

    public void FadeIn(Action onComplete, float duration = 0.5f)
    {
        tween?.Kill();
        panel.gameObject.SetActive(true);
        panel.color = new Color(0f, 0f, 0f, 1f);
        tween = panel.DOFade(0f, duration);
        tween.OnComplete(() =>
        {
            tween = null;
            panel.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void FadeOut(Action onComplete, float duration = 0.5f)
    {
        tween?.Kill();
        panel.gameObject.SetActive(true);
        panel.color = new Color(0f, 0f, 0f, 0f);
        tween = panel.DOFade(1f, duration);
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
        });
    }
}
