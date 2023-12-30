using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Mine : Trap
{
    [SerializeField]
    private int damage = 10;
    public override TrapType Type => TrapType.Mine;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer);
        executer.Damage(damage);
        noticeGroup.Add("地雷が炸裂した", Color.red);
        await UniTask.Yield(cancellationToken: token);
    }
}
