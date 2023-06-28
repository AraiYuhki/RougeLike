using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum CardType
{
    NormalAttack,@      // ’ÊíUŒ‚
    RangeAttack,        //@”ÍˆÍUŒ‚
    LongRangeAttack,    // ‰“‹——£UŒ‚
    RoomAttack,         // •”‰®‘S‘ÌUŒ‚
    Heal,               // ‰ñ•œ
    StaminaHeal,        // ƒXƒ^ƒ~ƒi‰ñ•œ
    Charge,             // UŒ‚—Í2”{(d•¡‚·‚é‚²‚Æ‚É”{—¦‚ª100“’Ç‰Á‚³‚êAÅ‘å‚Å4”{)
    ResourceAttack,     // ‚±‚ê‚Å‚Æ‚Ç‚ß‚ðŽh‚·‚Æ•K‚¸‚¨‹à‚ð—Ž‚Æ‚·‘ã‚í‚è‚ÉUŒ‚—Í‚ª’á‚ß
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
