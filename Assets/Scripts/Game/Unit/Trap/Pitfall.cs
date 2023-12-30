using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Pitfall : Trap
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Renderer[] renderers = new Renderer[0];
    public override TrapType Type => TrapType.Pitfall;

    protected override void Awake()
    {
    }

    public override void SetMaterials(Material wallMaterial, Material floorMaterial)
    {
        base.SetMaterials(wallMaterial, floorMaterial);
        foreach (var renderer in renderers)
            renderer.sharedMaterial = floorMaterial;
    }

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token)
    {
        var renderer = GetComponentInChildren<Renderer>();
        renderer.sharedMaterial = wallMaterial;
        var cancellationToken = CreateLinkedToken(token);
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
}
