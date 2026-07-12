using System;
using UnityEngine;

public enum AilmentType
{
    HandLock,   // 手札封じ
    Poison,     // 毒(HPスリップ)
    Exhaustion, // 疲労(腹減り追加)
    Bind,       // 拘束(移動不可)
    Paralysis,  // 麻痺(カード使用不可)
    Blind,      // 盲目(視認範囲減少)
}

public static class AilmentTypeExtensions
{
    public static string ToLabel(this AilmentType type)
    {
        return type switch
        {
            AilmentType.HandLock => "手札封じ",
            AilmentType.Poison => "毒",
            AilmentType.Exhaustion => "疲労",
            AilmentType.Bind => "拘束",
            AilmentType.Paralysis => "麻痺",
            AilmentType.Blind => "盲目",
            _ => type.ToString(),
        };
    }
}

[Serializable]
public class AilmentData
{
    [SerializeField]
    private AilmentType type;
    [SerializeField]
    private int param;
    [SerializeField]
    private int remainingTurn;

    public AilmentType Type => type;
    public int Param => param;

    public int RemainingTurn => remainingTurn;
    public bool IsInfinit => RemainingTurn <= -1;

    public AilmentData(AilmentType type, int param, int turn)
    {
        this.type = type;
        this.param = param;
        remainingTurn = turn;
    }

    public void SetData(int param, int turn)
    {
        this.param = param;
        remainingTurn = turn;
    }

    /// <summary>
    /// ターン経過させて、結果的に残りターン数がなくなればTrueを返す
    /// </summary>
    /// <returns></returns>
    public bool DecrementTurn ()
    {
        remainingTurn--;
        return RemainingTurn == 0;
    }

    public override string ToString()
    {
        if (IsInfinit)
            return $"{type}{param}(∞)";
        return $"{type}{param}{remainingTurn}";
    }

    public AilmentData Clone()
    {
        return new AilmentData(type, param, remainingTurn);
    }
}
