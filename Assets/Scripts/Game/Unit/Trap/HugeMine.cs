using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

public class HugeMine : Trap
{
    [SerializeField]
    private int damage = 10;
    public override TrapType Type => TrapType.HugeMine;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer);
        executer.Damage(damage);
        foreach (var unit in floorManager.GetAroundTilesAt(executer.Position).Select(tile => floorManager.GetUnit(tile.Position)).Where(unit => unit != null))
            unit.Damage(damage);
        noticeGroup.Add("大きな地雷が炸裂した", Color.red);
        await UniTask.Yield(cancellationToken: token);
    }

    public override void Execute(Unit executer, Action onComplete)
    {
        base.Execute(executer, onComplete);
        executer.Damage(damage);
        foreach (var unit in floorManager.GetAroundTilesAt(executer.Position).Select(tile => floorManager.GetUnit(tile.Position)).Where(unit => unit != null))
            unit.Damage(damage);
        noticeGroup.Add("大きな地雷が炸裂した", Color.red);
    }
}
