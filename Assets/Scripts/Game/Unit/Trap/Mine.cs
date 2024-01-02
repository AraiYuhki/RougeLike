using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Mine : Trap
{
    public override TrapType Type => TrapType.Mine;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer);
        noticeGroup.Add("地雷が炸裂した", Color.red);
        await UniTask.Yield(cancellationToken: token);
    }
}
