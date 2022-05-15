using DG.Tweening;
using UnityEngine;

public class Player : Unit
{
    public PlayerData Data { get; private set; } = new PlayerData(10);
    public override int Hp { get => Mathf.FloorToInt(Data.Hp); set => Data.Hp = value; }
    public override void AddExp(int exp) => Data.AddExp(exp);

    public bool IsLockInput { get; set; } = false;

    public void Initialize(int lv, int hp, int atk, int def)
    {
        Data = new PlayerData(hp)
        {
            Lv = lv,
            Atk = atk,
            Def = def,
        };
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
        Data.Hp += (Data.Stamina <= 0) ? -1 : Data.MaxHP * 0.095f;
        Data.Stamina = Mathf.Clamp(Data.Stamina, 0, Data.MaxStamina);
        Data.Hp = Mathf.Clamp(Data.Hp, 0, Data.MaxHP);
    }
}
