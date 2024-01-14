using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableItem : InteractiveItem
{
    [SerializeField]
    protected Button button;
    [SerializeField]
    protected TMP_Text label;

    public Button.ButtonClickedEvent OnClick { get => button.onClick; set => button.onClick = value; }
    public override bool Enable { get => button.interactable; set => button.interactable = value; }

    protected override float fadeDuration => button.colors.fadeDuration;

    public string Label
    {
        get => label.text;
        set => label.text = value;
    }

    protected override void OnEnable()
    {
        targetImage.color = isSelected ? selectedColor : normalColor;
    }

    public override void Initialize(Action onSelect = null, Action onSubmit = null)
    {
        this.onSelect = onSelect;
        button.onClick.AddListener(() => onSubmit?.Invoke());
    }

    public override void Submit() => button.onClick?.Invoke();
}
