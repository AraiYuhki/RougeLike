using System;
using UnityEngine;

[Serializable]
public class PlayerData : UnitData
{
    [SerializeField, HideInInspector]
    private float stamina = 100f;
    [SerializeField, HideInInspector]
    private int gems = 0;

    public float Stamina { get => stamina; set => stamina = Mathf.Clamp(value, 0, MaxStamina); }
    public float MaxStamina { get; set; } = 100f;
    public int Gems { get => gems; set => gems = Mathf.Max(0, value); }

    public PlayerData(int hp) : base(hp)
    {
    }
}
