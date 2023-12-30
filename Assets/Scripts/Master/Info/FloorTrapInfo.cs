using System;
using UnityEngine;

[Serializable]
public class FloorTrapInfo : ILotterable
{
    [SerializeField]
    private int groupId;
    [SerializeField, TrapReference]
    private int trapId;
    [SerializeField]
    private int probability;

    public int GroupId => groupId;
    public int TrapId => trapId;
    public int Probability => probability;
}
