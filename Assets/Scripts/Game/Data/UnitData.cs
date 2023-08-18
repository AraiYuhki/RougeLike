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
    [SerializeField]
    protected int atk = 10;

    public int TotalExp { get; set; } = 0;
    public virtual int MaxHP { get; protected set; } = 15;
    public float Hp { get; set; } = 15f;
    public int Lv { get; set; } = 1;
    public virtual int Atk { get => atk; set => atk = Mathf.Min(value, MaxAtk); }
    public virtual int Def { get; set; } = 0;
    public UnitData(int hp) => Hp = MaxHP = hp;
    public Dictionary<ItemBase, int> Inventory { get; protected set; } = new Dictionary<ItemBase, int>();
    public virtual void AddExp(int exp) => TotalExp += exp;

    public virtual void TakeItem(ItemBase item)
    {
        // スタックできるアイテムの場合は、インベントリに既に存在しているか確認し、存在している場合は所持数を加算
        if (item.IsStackable)
        {
            if (Inventory.Any(i => i.Key.Name == item.Name))
            {
                Inventory[item]++;
                return;
            }
        }
        // 新しくインベントリにアイテムを追加する
        if (Inventory.Count >= MaxInventorySize) throw new Exception("Inventory exceeded limit");
        Inventory.Add(item, 1);
    }
    public virtual void RemoveItem(ItemBase item)
    {
        Inventory.Remove(item);
    }
}

public partial class PlayerData : UnitData
{
    private float stamina = 100f;
    public float Stamina { get => stamina; set => stamina = Mathf.Clamp(value, 0, MaxStamina); }
    public float MaxStamina { get; set; } = 100f;
    private int gems = 0;
    public int Gems { get => gems; set => gems = Mathf.Max(0, value); }
    public WeaponData EquipmentWeapon { get; set; }
    public ShieldData EquipmentShield { get; set; }
    public int BaseAtk => levelData[Lv - 1].Atk;
    public int WeaponPower => EquipmentWeapon != null ? EquipmentWeapon.Atk + EquipmentWeapon.Lv : 0;
    public override int Def => EquipmentShield != null ? EquipmentShield.Def + EquipmentShield.Lv : 0;
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

[Serializable]
public class EnemyData : UnitData, ICloneable
{
    [SerializeField]
    private string name = string.Empty;
    [SerializeField]
    private int exp = 5;
    [SerializeField]
    private int maxHP = 10;
    [SerializeField]
    private int def = 0;
    
    public string Name { get => name; private set => name = value; }
    public override int Def { get => def; set => def = value; }
    public override int MaxHP { get => maxHP; protected set => maxHP = value; }
    public int Exp { get => exp; set => exp = value; }
    public EnemyData(int hp) : base(hp) { }

    public object Clone()
    {
        return new EnemyData(MaxHP)
        {
            Name = Name,
            MaxHP = MaxHP,
            Atk = Atk,
            Def = Def,
            Exp = Exp,
        };
    }
}
