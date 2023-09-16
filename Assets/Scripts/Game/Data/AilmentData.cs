using System;
using UnityEngine;

public enum AilmentType
{
    HandLock,   // ��D����
    Poison,     // ��(HP�X���b�v)
    Exhaustion, // ��J(������ǉ�)
    Bind,       // �S��(�ړ��s��)
    Paralysis,  // ���(�J�[�h�g�p�s��)
    Blind,      // �Ӗ�(���F�͈͌���)
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
    /// �^�[���o�߂����āA���ʓI�Ɏc��^�[�������Ȃ��Ȃ��True��Ԃ�
    /// </summary>
    /// <returns></returns>
    public bool DecrementTurn ()
    {
        RemainingTurn--;
        return RemainingTurn == 0;
    }
}
