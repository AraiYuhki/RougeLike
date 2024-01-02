using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BearTrap : Trap
{
    [SerializeField]
    private Animator animator;

    public override TrapType Type => TrapType.BearTrap;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer, token);
        var cancellationToken = CreateLinkedToken(token);
        DamageToExecuter(executer);
        noticeGroup.Add("プレイヤーはトラバサミを踏んだ", Color.red);
        await animator.PlayAsync(AnimatorHash.Execute, token: cancellationToken);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
        animator.PlayAsync(AnimatorHash.Release, token: cancellationToken).Forget();
    }
}
