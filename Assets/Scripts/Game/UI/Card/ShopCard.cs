using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCard : SelectableItem
{
    [SerializeField]
    private TMP_Text descriptionLabel;
    [SerializeField]
    private Image illust;
    [SerializeField]
    private TMP_Text priceLabel;

    public CardInfo Data { get; private set; }
    public Card Card { get; private set; }
    public int Price => Data.Price;

    public bool Enable
    {
        get => button.interactable;
        set => button.interactable = value;
    }

    public void SetData(CardInfo data, Card card, bool isRemove)
    {
        Card = card;
        Data = data;
        label.text = data.Name;
        descriptionLabel.text = data.Description;
        priceLabel.text = isRemove ? "200G" : $"{data.Price}G";
        illust.sprite = data.Illust;
    }

    public void Click() => button.onClick?.Invoke();

}
