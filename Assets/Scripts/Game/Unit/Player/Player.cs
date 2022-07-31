using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Player : Unit
{
    public PlayerData Data { get; private set; } = new PlayerData(10);
    public override int Hp { get => Mathf.FloorToInt(Data.Hp); set => Data.Hp = value; }
    public override int MaxHp { get => Data.MaxHP; }
    public override void AddExp(int exp) => Data.AddExp(exp);
    public override void RecoveryStamina(float value) => Data.Stamina += value;
    public override void PowerUp(int value) => Data.Atk += value;

    public bool IsLockInput { get; set; }

    public void Initialize(int lv, int hp, int atk, int def)
    {
        Data = new PlayerData(hp)
        {
            Lv = lv,
            Atk = atk,
            Def = def,
        };
        for (var i = 0; i < 20; i++)
        {
            Data.Inventory.Add(DataBase.Instance.GetTable<MItem>().Data.First().Clone() as ItemBase, 1);
        }
    }

    public override void Initialize()
    {
        Initialize(1, 10, 1, 1);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Move(Vector2Int move)
    {
        IsLockInput = true;
        Move(move, () => IsLockInput = false);
    }

    public override void TurnEnd()
    {
        Data.Stamina -= 0.1f;
        if (Data.Stamina <= 0)
            Damage(1, null);
        else
            Heal(Data.MaxHP * 0.095f);
    }
}
