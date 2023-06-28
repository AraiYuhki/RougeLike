using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum CardType
{
    NormalAttack,�@      // �ʏ�U��
    RangeAttack,        //�@�͈͍U��
    LongRangeAttack,    // �������U��
    RoomAttack,         // �����S�̍U��
    Heal,               // ��
    StaminaHeal,        // �X�^�~�i��
    Charge,             // �U����2�{(�d�����邲�Ƃɔ{����100���ǉ�����A�ő��4�{)
    ResourceAttack,     // ����łƂǂ߂��h���ƕK�������𗎂Ƃ�����ɍU���͂����
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
