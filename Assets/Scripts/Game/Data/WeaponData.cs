using System;
using UnityEngine;

[Serializable]
public class WeaponData : ItemBase
{
    [SerializeField]
    private int atk = 1;
    public int Atk { get => atk; set => atk = value; }
    public int Lv { get; set; } = 0;

    public WeaponData(WeaponData other) : base(other)
    {
        Atk = other.Atk;
        Lv = other.Lv;
    }

    public override object Clone() => new WeaponData(this);
}
