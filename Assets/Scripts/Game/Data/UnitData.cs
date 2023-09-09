using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public UnitData() { }
    public UnitData(int hp) => Hp = MaxHP = hp;
    public virtual void AddExp(int exp) => TotalExp += exp;
}

public partial class PlayerData : UnitData
{
    private float stamina = 100f;
    public float Stamina { get => stamina; set => stamina = Mathf.Clamp(value, 0, MaxStamina); }
    public float MaxStamina { get; set; } = 100f;
    private int gems = 0;
    public int Gems { get => gems; set => gems = Mathf.Max(0, value); }
    public int BaseAtk => levelData[Lv - 1].Atk;
    public int CurrentLevelExp => TotalExp - levelData[Lv - 1].RequireExp;
    public int NextLevelExp => levelData[Lv].RequireExp - levelData[Lv - 1].RequireExp;

    public PlayerData(int hp) : base(hp) 
    {
        Atk = 1;
    }
    public override void AddExp(int exp)
    {
        base.AddExp(exp);
        TotalExp = Mathf.Clamp(TotalExp, 0, levelData.Last().RequireExp);
        if (exp > 0)
        {
            while (TotalExp >= levelData[Lv].RequireExp)
            {
                MaxHP += UnityEngine.Random.Range(1, 6);
                Hp = MaxHP;
                Lv++;
            }
            return;
        }

        while(Lv > 1 && TotalExp < levelData[Lv - 2].RequireExp)
        {
            MaxHP -= UnityEngine.Random.Range(1, 6);
            MaxHP = Mathf.Max(15, MaxHP);
            Hp = MaxHP;
            Lv--;
        }
    }
}
