using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Pitfall : Trap
{
    [SerializeField]
    private Animator animator;
    public override TrapType Type => TrapType.Pitfall;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token)
    {
        var cancellationToken = CreateLinkedToken(token);
        IsVisible = true;
        await animator.PlayAsync(AnimatorHash.Execute, token: cancellationToken);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
        await animator.PlayAsync(AnimatorHash.Release, token: cancellationToken);
        if (executer is Enemy enemy)
        {
            enemy.Dead(null);
            noticeGroup.Add($"{enemy.Data.Name}は落とし穴に落ちた");
        }
        else if (executer is Player player)
        {
            player.Damage(10);
            noticeGroup.Add("プレイヤーは落とし穴に落ちた");
        }
    }

    public override async void Execute(Unit executer, Action onComplete)
    {
        IsVisible = true;
        await animator.PlayAsync(AnimatorHash.Execute);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await animator.PlayAsync(AnimatorHash.Release);
        if (executer is Enemy enemy)
        {
            enemy.Dead(null);
            noticeGroup.Add($"{enemy.Data.Name}は落とし穴に落ちた");
        }
        else if (executer is Player player)
        {
            player.Damage(10);
            noticeGroup.Add("プレイヤーは落とし穴に落ちた");
        }
        onComplete?.Invoke();
    }
}
