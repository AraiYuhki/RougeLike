using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[Serializable]
public class PlayerData : UnitData
{
    [SerializeField]
    private float hp;
    [SerializeField, HideInInspector]
    private LimitedParam stamina = new LimitedParam(100f);
    [SerializeField, HideInInspector]
    private int gems = 0;
    [SerializeField]
    private float chargeStack = 0;
    [SerializeField]
    private int healInterval = 0;

    public override string Name => "プレイヤー";
    public override float Hp { get => hp; set => hp = value; }
    public override int MaxHP => 10;
    public float Stamina { get => stamina; set => stamina.Value = value; }
    public float MaxStamina { get; set; } = 100f;
    public int Gems { get => gems; set => gems = Mathf.Max(0, value); }
    public float ChargeStack { get => chargeStack; set => chargeStack = value; }
    public int HealInterval { get => healInterval; set => healInterval = value; }
    public override int Def => 0;

    public PlayerData() { }

    public PlayerData(float hp)
    {
        this.hp = hp;
    }

    public PlayerData Clone()
    {
        var newData = new PlayerData(hp)
        {
            position = position,
            angle = angle,
            stamina = new LimitedParam(stamina, stamina.Max),
            gems = gems,
            atk = atk,
            chargeStack = chargeStack,
            healInterval = healInterval
        };
        foreach (var pair in ailments)
            newData.ailments[pair.Key] = pair.Value.Clone();

        return newData;
    }
}
