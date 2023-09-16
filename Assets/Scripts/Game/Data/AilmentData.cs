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

[Serializable]
public class AilmentData
{
    public AilmentType Type { get; private set; }
    public int Param { get; private set; }

    public int RemainingTurn { get; private set; }
    public bool IsInfinit => RemainingTurn <= -1;

    public AilmentData(AilmentType type, int param, int turn)
    {
        Type = type;
        Param = param;
        RemainingTurn = turn;
    }

    public void SetTurn(int turn)
    {
        RemainingTurn = Mathf.Max(RemainingTurn, turn);
    }

    /// <summary>
    /// ターン経過させて、結果的に残りターン数がなくなればTrueを返す
    /// </summary>
    /// <returns></returns>
    public bool DecrementTurn ()
    {
        RemainingTurn--;
        return RemainingTurn == 0;
    }
}
