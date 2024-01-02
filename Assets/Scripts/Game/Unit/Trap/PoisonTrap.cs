using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class PoisonTrap : Trap
{
    public override TrapType Type => TrapType.PoisonTrap;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer);
        noticeGroup.Add($"{executer.Name}は毒菱を踏んだ", Color.red);
        await UniTask.Yield(cancellationToken: token);
    }
}
