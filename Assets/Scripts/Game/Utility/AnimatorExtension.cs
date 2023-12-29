using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public static class AnimatorExtension
{
    public static async UniTask PlayAsync(this Animator self, int hash, int layer = 0, CancellationToken token = default)
    {
        self.Play(hash, layer);
        await UniTask.Yield(cancellationToken: token);
        var stateInfo = self.GetCurrentAnimatorStateInfo(layer);
        await UniTask.Delay(TimeSpan.FromSeconds(stateInfo.length), cancellationToken: token);
    }
}
