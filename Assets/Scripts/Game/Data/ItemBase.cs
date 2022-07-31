using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemBase : ICloneable
{
    [SerializeField]
    private string name = string.Empty;
    [SerializeField]
    private int buyPrice = 100;
    [SerializeField]
    private int sellPrice = 50;
    public string Name { get => name; set => name = value; }
    public int BuyPrice { get => buyPrice; set => buyPrice = value; }
    public int SellPrice { get => sellPrice; set => sellPrice = value; }
    public virtual bool IsStackable => false;

    public ItemBase() { }
    public ItemBase(ItemBase other)
    {
        name = other.name;
        buyPrice = other.buyPrice;
        sellPrice = other.sellPrice;
    }

    public virtual object Clone() => new ItemBase(this);
}
