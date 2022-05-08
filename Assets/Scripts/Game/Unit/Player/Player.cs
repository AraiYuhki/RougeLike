using DG.Tweening;
using UnityEngine;

public class Player : Unit
{
    public PlayerData Data { get; private set; } = new PlayerData(10);

    public bool IsLockInput { get; set; } = false;

    public void Initialize(int lv, int hp, int atk, int def)
    {
        Data = new PlayerData(hp)
        {
            Lv = lv,
            Atk = atk,
            Def = def,
            TotalExp = 0,
            MaxStamina = 100,
            Stamina = 100f
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
}
