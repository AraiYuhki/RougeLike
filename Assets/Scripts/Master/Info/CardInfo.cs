using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CardType
{
    NormalAttack,       // 通常攻撃
    RangeAttack,        //　範囲攻撃
    LongRangeAttack,    // 遠距離攻撃
    RoomAttack,         // 部屋全体攻撃
    Heal,               // 回復
    StaminaHeal,        // スタミナ回復
    Charge,             // 攻撃力2倍(重複するごとに倍率が100％追加され、最大で4倍)
    ResourceAttack,     // これで与えたダメージ分のお金を落とす

    DrawAndUse,         //　指定数のカードを引いて、そのまま使用する

    Passive,
}

public enum CardCategory
{
    Attack,     // 攻撃系
    Utility,    // アイテム系
    Passive,    // 装備系
    Special,    //　特殊系(連続攻撃など)
}

[Serializable]
public class CardInfo
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("name")]
    private string name;
    [SerializeField, CsvColumn("type")]
    private CardType type;
    [SerializeField, CsvColumn("param1")]
    private float param1;
    [SerializeField, CsvColumn("param2")]
    private int param2;
    [SerializeField, CsvColumn("price")]
    private int price;
    [SerializeField, CsvColumn("category")]
    private CardCategory category;
    [SerializeField, CsvColumn("attackAreaId")]
    private int attackAreaDataId = -1;
    [SerializeField, CsvColumn("passiveEffectId")]
    private int passiveEffectId = -1;

    private AttackAreaInfo attackAreaData;

    public int Id => id;

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

    public CardCategory Category
    {
        get => category;
        set => category = value;
    }

    public float Param1
    {
        get => param1;
        set => param1 = value;
    }

    public int Param2
    {
        get => param2;
        set => param2 = value;
    }

    public int Range
    {
        get => param2;
        set => param2 = value;
    }

    public CardCategory TargetCategory
    {
        get => (CardCategory)param2;
        set => param2 = (int)value;
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

    public AttackAreaInfo AttackAreaData
    {
        get
        {
            if (attackAreaData == null)
                attackAreaData = DB.Instance.MAttackArea.GetById(attackAreaDataId);
            return attackAreaData;
        }
    }

    public bool IsPassive => passiveEffectId >= 0;
    public int PassiveEffectId
    {
        get => passiveEffectId;
        set => passiveEffectId = value;
    }

    public virtual CardInfo Clone()
    {
        return new CardInfo
        {
            Name = Name,
            Type = Type,
            Param1 = Param1,
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
