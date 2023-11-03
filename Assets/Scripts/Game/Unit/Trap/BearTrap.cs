using Cysharp.Threading.Tasks;
using System;
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

    public  override async void Execute(Unit executer)
    {
        base.Execute(executer);
        executer.Damage(damage);
        if (executer is Player player)
        {
            player.Data.AddAilment(AilmentType.Bind, 0, turn);
            noticeGroup.Add("プレイヤーはトラバサミを踏んだ", Color.red);
        }
        else if (executer is Enemy enemy)
        {
            enemy.Data.AddAilment(AilmentType.Bind, 0, turn);
            noticeGroup.Add($"{enemy.Name}はトラバサミを踏んだ", Color.red);
        }
        await animator.PlayAsync(AnimatorHash.Execute);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await animator.PlayAsync(AnimatorHash.Release);
    }
}
