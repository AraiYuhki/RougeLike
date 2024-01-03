using System;
using UnityEngine;

/// <summary>
/// 最大値付きパラメータクラス
/// できるだけ簡素化するための措置
/// </summary>
[Serializable]
public class LimitedParam
{
    [SerializeField]
    private float max = 0f;
    [SerializeField, HideInInspector]
    private float value = 0f;

    public LimitedParam() { }
    public LimitedParam(float value) => this.value = max = value;
    public LimitedParam(float value, float max)
    {
        this.max = max;
        this.value = Mathf.Clamp(value, 0f, max);
    }

    public float Max { get => max; set => max = value; }
    public float Value
    {
        get => value;
        set => this.value = Mathf.Clamp(value, 0f, max);
    }

    public bool IsMax => value >= max;

    public float NormalizedValue => value / max;

    public static implicit operator float(LimitedParam self) => self.value;
    public static explicit operator int(LimitedParam self) => (int)self.value;

    public static LimitedParam operator +(LimitedParam left, float right)
        => new LimitedParam(left.value + right, left.max);
    public static LimitedParam operator -(LimitedParam left, float right)
        => new LimitedParam(left.value - right, left.max);
    public static LimitedParam operator *(LimitedParam left, float right)
        => new LimitedParam(left.value * right, left.max);
    public static LimitedParam operator /(LimitedParam left, float right)
        => new LimitedParam(left.value / right, left.max);

    public static bool operator <(LimitedParam left, float right)
        => left.value < right;
    public static bool operator >(LimitedParam left, float right)
        => left.value > right;
    public static bool operator <=(LimitedParam left, float right)
        => left.value <= right;
    public static bool operator >=(LimitedParam left, float right)
        => left.value >= right;

    public static bool operator <(float left, LimitedParam right)
        => left < right.value;
    public static bool operator >(float left, LimitedParam right)
        => left > right.value;
    public static bool operator <=(float left, LimitedParam right)
        => left <= right.value;
    public static bool operator >=(float left, LimitedParam right)
        => left >= right.value;
}
