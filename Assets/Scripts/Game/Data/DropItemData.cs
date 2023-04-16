using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItemData
{
    [SerializeField]
    private int probability = 10;
    [SerializeField]
    private ItemType type;
    [SerializeField]
    private int id = 0;

    public int Probability => probability;
    public ItemType Type => type;
    public int Id => id;
}
