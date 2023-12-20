using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class UnitData
{
    public const int MaxInventorySize = 20;
    public const int MaxAtk = 8;
    [SerializeField, CsvColumn("atk")]
    protected int atk = 10;

    public int TotalExp { get; set; } = 0;
    public virtual int MaxHP { get; protected set; } = 15;
    public float Hp { get; set; } = 15f;
    public int Lv { get; set; } = 1;
    public virtual int Atk { get => atk; set => atk = Mathf.Min(value, MaxAtk); }
    public virtual int Def { get; set; } = 0;

    public List<AilmentData> Ailments { get; private set; } = new List<AilmentData>();

    public UnitData(int hp) => Hp = MaxHP = hp;

    public void AddAilment(AilmentType type, int param, int turn)
    {
        var duplicateData = Ailments.FirstOrDefault(ailment => ailment.Type == type);
        if (duplicateData != null && duplicateData.Param > param)
        {
            duplicateData.SetTurn(turn);
            return;
        }
        var newData = new AilmentData(type, param, turn);
        Ailments.Add(newData);
    }

    public int GetAilmengEffects(AilmentType type)
    {
        return Ailments.Where(ailment => ailment.Type == type).Sum(ailment => ailment.Param);
    }
}
