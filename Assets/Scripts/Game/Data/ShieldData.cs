using System;
using UnityEngine;

[Serializable]
public class ShieldData : ItemBase
{
    [SerializeField]
    private int def = 1;
    public int Def { get => def; set => def = value; }
    public int Lv { get; set; } = 0;

    public ShieldData(ShieldData other) : base(other)
    {
        def = other.Def;
        Lv = other.Lv;
    }

    public override object Clone() => new ShieldData(this);
}
