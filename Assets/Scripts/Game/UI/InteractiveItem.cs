using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractiveItem : MonoBehaviour
{
    [SerializeField]
    protected Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField]
    protected Color selectedColor = new Color(0f, 1f, 1f, 0.5f);
    [SerializeField]
    protected Image targetImage;

    protected bool isSelected = false;
    protected Tween tween;
    protected Action onSelect;
    protected Action onSubmit;
    protected abstract float fadeDuration { get; }
    public virtual bool Enable { get; set; }
    public virtual void Select(bool isSelected)
    {
        if (this.isSelected == isSelected) return;
        this.isSelected = isSelected;
        ChangeColor();
    }

    protected virtual void OnEnable()
    {
        targetImage.color = isSelected ? selectedColor : normalColor;
    }

    protected virtual void ChangeColor()
    {
        if (tween != null)
        {
            tween.Complete();
            tween = null;
        }
        var destinationColor = isSelected ? selectedColor : normalColor;
        tween = targetImage.DOColor(destinationColor, fadeDuration);
        tween.OnComplete(() => tween = null);
        tween.Play();
    }

    public virtual void Initialize(Action onSelect = null, Action onSubmit = null)
    {
        this.onSelect = onSelect;
    }

    public void OnHover() => onSelect?.Invoke();
    public void OnMouseEnter() => onSelect?.Invoke();
    public virtual void Right() { }
    public virtual void Left() { }
    public virtual void Up() { }
    public virtual void Down() { }
    public virtual void Submit() => onSubmit?.Invoke();
    protected void OnDestroy()
    {

    }
}
