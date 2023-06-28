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
}

[Serializable]
public class CardData : ICloneable
{
    [SerializeField]
    private string name;
    [SerializeField]
    private CardType type;
    [SerializeField]
    private float param;
    [SerializeField]
    private int range;
    [SerializeField]
    private int price;
    [SerializeField]
    private int attackAreaDataId = -1;

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

    public AttackAreaData AttackAreaData
    {
        get
        {
            if (attackAreaDataId < 0) return null;
            return DataBase.Instance.GetTable<MAttackArea>().GetData(attackAreaDataId);
        }
    }

    //public AttackAreaData AttackAreaData
    //{
    //    get => attackAreaData;
    //    set => attackAreaData = value;
    //}
    

    public virtual object Clone()
    {
        throw new NotImplementedException();
    }

    public virtual bool CanUse(Unit user)
    {
        return true;
    }
}
