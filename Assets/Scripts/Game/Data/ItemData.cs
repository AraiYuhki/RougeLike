using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string Name { get; set; }
    public int BuyPrive { get; set; } = 100;
    public int SellPrive { get; set; } = 50;
}
