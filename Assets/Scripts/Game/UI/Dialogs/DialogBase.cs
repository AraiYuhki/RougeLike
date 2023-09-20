using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DialogType
{
    Unknown,
    Common,
}

public class DialogBase : MonoBehaviour, IControllable
{
    [SerializeField]
    protected Image window;
    [SerializeField]
    protected CanvasGroup canvasGroup;
    [SerializeField]
    private TMP_Text title;

    public virtual DialogType Type => throw new NotImplementedException();

    protected int currentSelected = 0;

    public Action OnDestroyed { get; set; }
    protected bool lockInput = true;
    protected Sequence tween;
    public string Title
    {
        get => title.text;
        set => title.text = value;
    }

    public virtual void Open(Action onComplete = null)
    {
        tween?.Kill();

        window.rectTransform.localScale = Vector3.one * 0.5f;
        canvasGroup.alpha = 0f;

        gameObject.SetActive(true);

        tween = DOTween.Sequence();
        tween.Append(window.rectTransform.DOScale(Vector3.one, 0.2f));
        tween.Join(canvasGroup.DOFade(1.0f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
            lockInput = false;
        });
    }

    public virtual void Close(Action onComplete = null)
    {
        tween?.Kill();

        window.rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        lockInput = true;

        tween = DOTween.Sequence();
        tween.Append(window.rectTransform.DOScale(0.5f, 0.2f));
        tween.Join(canvasGroup.DOFade(0f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
        });
    }

    public virtual void Controll()
    {
    }

    public virtual void Left() { }
    public virtual void Right() { }
    public virtual void Up() { }
    public virtual void Down() { }
    public virtual void Submit() { }
    public virtual void Cancel() { }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
        tween?.Kill();
        tween = null;
    }

}
