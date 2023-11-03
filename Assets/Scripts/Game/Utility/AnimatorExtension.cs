using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public static class AnimatorExtension
{
    public static async UniTask PlayAsync(this Animator self, string stateName, int layer = 0)
    {
        self.Play(stateName, layer);
        await UniTask.Yield();
        var stateInfo = self.GetCurrentAnimatorStateInfo(layer);
        await UniTask.Delay(TimeSpan.FromSeconds(stateInfo.length));
    }

    public static async UniTask PlayAsync(this Animator self, int hash, int layer = 0)
    {
        self.Play(hash, layer);
        await UniTask.Yield();
        var stateInfo = self.GetCurrentAnimatorStateInfo(layer);
        await UniTask.Delay(TimeSpan.FromSeconds(stateInfo.length));
    }
}
