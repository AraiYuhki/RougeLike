using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Pitfall : Trap
{
    [SerializeField]
    private Animator animator;
    public override TrapType Type => TrapType.Pitfall;

    public override async void Execute(Unit executer)
    {
        base.Execute(executer);
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
    }
}
