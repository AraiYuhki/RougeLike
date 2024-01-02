using System;
using UnityEngine;

public enum ShopCardType
{
    GotoOther,
    Exit
}

public class SelectableCard : SelectableItem
{
    [SerializeField]
    private ShopCardType type;
    public ShopCardType Type => type;

    public void Setup(string label, ShopCardType type)
    {
        Label = label;
        this.type = type;
    }
    public void SetType(ShopCardType type) => this.type = type;

    public Action OnClick { get; set; }
    public bool Enable
    {
        get => button.interactable;
        set => button.interactable = value;
    }
}
