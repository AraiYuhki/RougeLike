using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PoisonTrap : Trap
{
    [SerializeField]
    private int power = 1;
    [SerializeField]
    private int turn = 10;
    public override TrapType Type => TrapType.PoisonTrap;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer);
        if (executer is Player player)
        {
            player.Data.AddAilment(AilmentType.Poison, power, turn);
            noticeGroup.Add("プレイヤーは毒菱を踏んだ", Color.red);
        }
        await UniTask.Yield(cancellationToken: token);
    }

    public override void Execute(Unit executer, Action onComplete)
    {
        base.Execute(executer, onComplete);
        if (executer is Player player)
        {
            player.Data.AddAilment(AilmentType.Poison, power, turn);
            noticeGroup.Add("プレイヤーは毒菱を踏んだ", Color.red);
        }
    }

}
