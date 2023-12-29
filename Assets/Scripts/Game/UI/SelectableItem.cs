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
    protected Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField]
    protected Color selectedColor = new Color(0f, 1f, 1f, 0.5f);
    [SerializeField]
    protected Button button;
    [SerializeField]
    protected TMP_Text label;

    protected bool isSelected = false;
    protected Action onSelect = null;
    protected Action onSubmit = null;
    protected Tween tween;

    public string Label
    {
        get => label.text;
        set => label.text = value;
    }

    private void OnEnable()
    {
        button.image.color = isSelected ? selectedColor : normalColor;
    }

    public virtual void Initialize(Action onSelect = null, Action onSubmit = null)
    {
        this.onSelect = onSelect;
        button.onClick.AddListener(() => onSubmit?.Invoke());
    }

    public void Select(bool isSelect)
    {
        if (isSelected == isSelect) return;
        if (tween != null)
        {
            tween.Complete();
            tween = null;
        }
        var destinationColor = isSelect ? selectedColor : normalColor;
        tween = button.image.DOColor(destinationColor, button.colors.fadeDuration);
        tween.OnComplete(() => tween = null);
        isSelected = isSelect;
        tween.Play();
    }

    public void OnHover()
    {
        onSelect?.Invoke();
    }

    public void OnMouseEnter()
    {
        onSelect?.Invoke();
    }

    public void Submit() => button.onClick?.Invoke();
    protected void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }
}
