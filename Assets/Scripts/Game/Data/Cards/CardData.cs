using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum CardType
{
    NormalAttack,　      // 通常攻撃
    RangeAttack,        //　範囲攻撃
    LongRangeAttack,    // 遠距離攻撃
    RoomAttack,         // 部屋全体攻撃
    Heal,               // 回復
    StaminaHeal,        // スタミナ回復
    Charge,             // 攻撃力2倍(重複するごとに倍率が100％追加され、最大で4倍)
    ResourceAttack,     // これでとどめを刺すと必ずお金を落とす代わりに攻撃力が低め

    Passive,
}

[Serializable]
public class CardData : ICloneable
{
    [SerializeField, CsvColumn("Name")]
    private string name;
    [SerializeField, CsvColumn("Type")]
    private CardType type;
    [SerializeField, CsvColumn("Param")]
    private float param;
    [SerializeField, CsvColumn("Range")]
    private int range;
    [SerializeField, CsvColumn("Price")]
    private int price;
    [SerializeField, CsvColumn("AttackAreaId")]
    private int attackAreaDataId = -1;
    [SerializeField, CsvColumn("PassiveEffectId")]
    private int passiveEffectId = -1;

    public string Name
    {
        get => name;
        set => name = value;
    }

    public virtual CardType Type
    {
        get => type;
        set => type = value;
    }

    public float Param
    {
        get => param;
        set => param = value;
    }

    public int Range
    {
        get => range;
        set => range = value;
    }

    public int Price
    {
        get => price;
        set => price = value;
    }

    public int AttackAreaDataId
    {
        get => attackAreaDataId;
        set => attackAreaDataId = value;
    }

    public bool IsPassive => passiveEffectId >= 0;
    public int PassiveEffectId
    {
        get => passiveEffectId;
        set => passiveEffectId = value;
    }

    public AttackAreaData AttackAreaData
    {
        get
        {
            if (attackAreaDataId < 0) return null;
            return DataBase.Instance.GetTable<MAttackArea>().GetData(attackAreaDataId);
        }
    }
    

    public virtual object Clone()
    {
        return new CardData
        {
            Name = Name,
            Type = Type,
            Param = Param,
            Range = Range,
            Price = Price,
            AttackAreaDataId = AttackAreaDataId,
            PassiveEffectId = PassiveEffectId,
        };
    }

    public virtual bool CanUse(Unit user)
    {
        return true;
    }
}
