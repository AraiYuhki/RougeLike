using System;
using UnityEngine;

public enum PassiveEffectType
{
    DefenceUp, // 防御アップ(固定値)
    DeffenceUpRate, // 防御アップ(割合軽減)
    AttackUp, // 攻撃アップ(固定値)
    AttackUpRate, // 攻撃アップ(割合上昇)
    DropUp, // 一定確率でジェムドロップ(リソースアタックと同じくらいの量)
    Counter, // 反撃(固定値)
    Refrect, // 反撃(受けたダメージのN%反撃)
    Satiated, // 腹減り軽減
}

[Serializable]
public class PassiveEffectData : ICloneable
{
    [SerializeField, CsvColumn("Id")]
    private int id;
    [SerializeField, CsvColumn("EffectType")]
    private PassiveEffectType effectType;
    [SerializeField, CsvColumn("Param1")]
    private int param1 = 0;
    [SerializeField, CsvColumn("Param2")]
    private int param2 = 0;

    public int Id => id;
    public PassiveEffectType EffectType => effectType;
    public int Param1 => param1;
    public int Param2 => param2;

    public object Clone()
    {
        return new PassiveEffectData()
        {
            id = id,
            effectType = effectType,
            param1 = param1,
            param2 = param2
        };
    }
}
