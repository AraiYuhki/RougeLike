using UnityEngine;

public class PlayerData : UnitData
{
    private float stamina = 100f;
    public float Stamina { get => stamina; set => stamina = Mathf.Clamp(value, 0, MaxStamina); }
    public float MaxStamina { get; set; } = 100f;
    private int gems = 0;
    public int Gems { get => gems; set => gems = Mathf.Max(0, value); }

    public PlayerData(int hp) : base(hp)
    {
    }
}
