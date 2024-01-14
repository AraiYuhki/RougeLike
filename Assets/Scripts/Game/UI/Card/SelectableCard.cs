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
}
