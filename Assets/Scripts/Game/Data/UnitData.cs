using System;
using UnityEngine;

[Serializable]
public abstract class UnitData
{
    public const int MaxInventorySize = 20;
    public const int MaxAtk = 8;
    [SerializeField, CsvColumn("name")]
    protected string name;
    [SerializeField, HideInInspector]
    protected float hp = 15f;
    [SerializeField, CsvColumn("atk")]
    protected int atk = 10;

    [SerializeField, HideInInspector]
    private Encyclopedia<AilmentType, AilmentData> ailments = new();

    public virtual string Name => name;
    public virtual int MaxHP { get; protected set; } = 15;
    public float Hp { get => hp; set => hp = value; }
    public int Lv { get; set; } = 1;
    public virtual int Atk { get => atk; set => atk = Mathf.Min(value, MaxAtk); }
    public virtual int Def { get; set; } = 0;

    public Encyclopedia<AilmentType, AilmentData> Ailments => ailments;

    public UnitData(int hp) => Hp = MaxHP = hp;

    public void AddAilment(AilmentType type, int param, int turn)
    {
        if (ailments.TryGetValue(type, out var value))
        {
            value.SetData(Mathf.Max(value.Param, param), turn);
            return;
        }
        ailments.Add(type, new AilmentData(type, param, turn));
    }

    public int GetAilmengEffects(AilmentType type)
    {
        return ailments.TryGetValue(type, out var value) ? value.Param : 0;
    }
}
