using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCard : SelectableItem
{
    [SerializeField]
    private Image illust;
    [SerializeField]
    private TMP_Text priceLabel;

    private CardData data;

    public int Price => data.Price;

    public bool Enable
    {
        get => button.interactable;
        set => button.interactable = value;
    }

    public void SetData(CardData data, Action onSelect = null, Action onClick = null)
    {
        this.data = data;
        label.text = data.Name;
        priceLabel.text = $"{data.Price}G";
        Initialize(onSelect, () => onClick?.Invoke());
    }

    public void Click() => button.onClick?.Invoke();

}
