using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BearTrap : Trap
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private int damage = 5;
    [SerializeField]
    private int turn = 5;

    public override TrapType Type => TrapType.BearTrap;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        var cancellationToken = CreateLinkedToken(token);
        IsVisible = true;
        executer.Damage(damage);
        if (executer is Player player)
        {
            player.Data.AddAilment(AilmentType.Bind, damage, turn);
            noticeGroup.Add("プレイヤーはトラバサミを踏んだ", Color.red);
        }
        await animator.PlayAsync(AnimatorHash.Execute, token: cancellationToken);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
        animator.PlayAsync(AnimatorHash.Release, token: cancellationToken).Forget();
    }

    public  override async void Execute(Unit executer, Action onComplete)
    {
        IsVisible = true;
        executer.Damage(damage);
        if (executer is Player player)
        {
            player.Data.AddAilment(AilmentType.Bind, damage, turn);
            noticeGroup.Add("プレイヤーはトラバサミを踏んだ", Color.red);
        }
        await animator.PlayAsync(AnimatorHash.Execute);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        onComplete?.Invoke();
        await animator.PlayAsync(AnimatorHash.Release);
    }
}
