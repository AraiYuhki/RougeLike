using UnityEngine;

public class Player : Unit
{
    public PlayerData Data { get; private set; }

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
        if (Mathf.Abs((DestPosition - transform.localPosition).magnitude) < 0.001f)
        {
            IsLockInput = false;
        }
    }

    public override void Move(Vector2Int move)
    {
        IsLockInput = true;
        base.Move(move);
    }
}
