using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;
using System.ComponentModel;
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

    PenetrateAttack,    // 貫通攻撃(直線上の敵全てにダメージ)
    AilmentAttack,      // 状態異常攻撃(前方の敵にダメージ+状態異常を付与)
    CureAilment,        // 状態異常をすべて回復
    Redraw,             // 手札をすべて捨てて引き直す
    Swap,               // 前方の敵と位置を入れ替える
    KnockbackAttack,    // ノックバック攻撃(吹き飛ばし、激突時に追加ダメージ)
    PullAttack,         // 引き寄せ攻撃(直線上の敵を目の前まで引き寄せて攻撃)
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
    [SerializeField, CsvColumn("description")]
    private string description;
    [SerializeField, CsvColumn("type")]
    private CardType type;
    [SerializeField, CsvColumn("param1")]
    private float param1;
    [SerializeField, CsvColumn("param2")]
    private int param2;
    [SerializeField, CsvColumn("param3")]
    private int param3;
    [SerializeField, CsvColumn("param4")]
    private int param4;
    [SerializeField, CsvColumn("price")]
    private int price;
    [SerializeField, CsvColumn("category")]
    private CardCategory category;
    [SerializeField, CsvColumn("attackAreaId")]
    private int attackAreaDataId = -1;
    [SerializeField, CsvColumn("passiveEffectId")]
    private int passiveEffectId = -1;
    [SerializeField, CsvColumn("illust")]
    private Sprite illust;

    private AttackAreaInfo attackAreaData = null;

    public int Id => id;

    public string Name
    {
        get => name;
        set => name = value;
    }

    public string Description
    {
        get => description;
        set => description = value;
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

    public int Param3
    {
        get => param3;
        set => param3 = value;
    }

    public int Param4
    {
        get => param4;
        set => param4 = value;
    }

    public int Range
    {
        get => param2;
        set => param2 = value;
    }

    // 状態異常攻撃用のエイリアス
    public AilmentType Ailment => (AilmentType)param2;
    public int AilmentParam => param3;
    public int AilmentTurn => param4;

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

    public Sprite Illust
    {
        get => illust;
        set => illust = value;
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
            Description = Description,
            Type = Type,
            Category = Category,
            Param1 = Param1,
            Param2 = Param2,
            Param3 = Param3,
            Param4 = Param4,
            Price = Price,
            AttackAreaDataId = AttackAreaDataId,
            PassiveEffectId = PassiveEffectId,
            Illust = Illust,
        };
    }

    public virtual bool CanUse(Unit user)
    {
        return true;
    }
}
