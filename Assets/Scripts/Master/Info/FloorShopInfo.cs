using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FloorShopInfo
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("sell_all")]
    public bool isSellAll = false;
    [SerializeField, CsvColumn("card_ids")]
    private List<int> cards = new List<int>();

    public int Id => id;
    public bool IsSellAll => isSellAll;
    public List<int> Cards => cards;
}
