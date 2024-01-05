using System;
using UnityEngine;

[Serializable]
public abstract class UnitData
{
    public const int MaxInventorySize = 20;
    public const int MaxAtk = 8;
    [SerializeField]
    protected Vector2Int position;
    [SerializeField]
    protected Vector2Int angle;
    [SerializeField]
    protected int atk = 10;

    [SerializeField, HideInInspector]
    protected Encyclopedia<AilmentType, AilmentData> ailments = new();

    public Vector2Int Position { get => position; set => position = value; }
    public Vector2Int Angle { get => angle; set => angle = value; }
    public abstract string Name { get; }
    public abstract int MaxHP { get; }
    public abstract float Hp { get; set; }
    public abstract int Def { get; }
    public virtual int Atk { get => atk; set => atk = Mathf.Min(value, MaxAtk); }

    public Encyclopedia<AilmentType, AilmentData> Ailments => ailments;

    public UnitData() { }

    public void AddAilment(AilmentType type, int param, int turn)
    {
        if (ailments.TryGetValue(type, out var value))
        {
            value.SetData(Mathf.Max(value.Param, param), turn);
            return;
        }
        ailments.Add(type, new AilmentData(type, param, turn));
    }

    public int GetAilmengEffects(AilmentType type)
    {
        return ailments.TryGetValue(type, out var value) ? value.Param : 0;
    }
}
