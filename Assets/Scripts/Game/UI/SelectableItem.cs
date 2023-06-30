using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableItem : MonoBehaviour
{
    [SerializeField]
    protected Button button;
    [SerializeField]
    protected TMP_Text label;

    protected bool isSelected = false;
    protected Action onSelect = null;
    protected Action onSubmit = null;
    protected Color initialColor = Color.white;
    protected Tween tween;

    public string Label
    {
        get => label.text;
        set => label.text = value;
    }

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
        tween.Play();
    }

    public void OnHover()
    {
        onSelect?.Invoke();
    }

    public void Submit() => button.onClick?.Invoke();
    protected void OnDestroy() => tween?.Kill();
}
