using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableItem : MonoBehaviour
{
    [SerializeField]
    protected Button button;

    protected bool isSelected = false;
    protected Action onSelect = null;
    protected Action onSubmit = null;
    protected Color initialColor = Color.white;
    protected Tween tween;

    private void OnEnable() => initialColor = button.image.color;
    public virtual void Initialize(Action onSelect = null, UnityAction onSubmit = null)
    {
        this.onSelect = onSelect;
        button.onClick.AddListener(onSubmit);
    }

    public void Select(bool isSelect)
    {
        if (isSelected == isSelect) return;
        if (tween != null)
        {
            tween.Complete();
            tween = null;
        }
        var destinationColor = isSelect ? button.colors.highlightedColor : button.colors.normalColor;
        destinationColor *= initialColor;
        tween = button.image.DOColor(destinationColor, button.colors.fadeDuration);
        tween.OnComplete(() => tween = null);
        isSelected = isSelect;
    }

    public void Submit() => button.onClick?.Invoke();
    public void OnMouseOver() => onSelect?.Invoke();
    protected void OnDestroy() => tween?.Kill();
}
