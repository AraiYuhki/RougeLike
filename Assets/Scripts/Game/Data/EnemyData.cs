using System;
using UnityEngine;

[Serializable]
public class EnemyData : UnitData
{
    [SerializeField]
    private int masterId;
    [SerializeField]
    private bool isEncouted = false;
    [SerializeField]
    private LimitedParam hp = new LimitedParam(15f);
    [SerializeField]
    private int def;
    [SerializeField]
    private float chargeStack;

    private EnemyInfo master;

    public int Id => masterId;
    public override string Name => Master.Name;
    public override float Hp { get => hp; set => hp.Value = value; }
    public override int MaxHP => (int)hp.Max;
    public override int Atk => atk;
    public override int Def => def;
    public bool IsEncouted { get => isEncouted; set => isEncouted = value; }
    public float ChargeStack { get => chargeStack; set => chargeStack = value; }
    public EnemyInfo Master
    {
        get
        {
            if (master == null)
                master = DB.Instance.MEnemy.GetById(masterId);
            return master;
        }
    }

    public EnemyData(float hp)
    {
        hp = new LimitedParam(hp);
    }
    public EnemyData(int masterId)
    {
        this.masterId = masterId;
        hp = new LimitedParam(Master.Hp);
        atk = Master.Atk;
        def = Master.Def;
    }

    public EnemyData Clone()
    {
        var newData = new EnemyData(masterId);
        foreach (var pair in ailments)
            newData.ailments[pair.Key] = pair.Value.Clone();
        return newData;
    }
}
