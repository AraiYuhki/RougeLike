using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;

public class HugeMine : Trap
{
    public override TrapType Type => TrapType.HugeMine;

    public override async UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        await base.ExecuteAsync(executer);
        foreach (var unit in floorManager.GetAroundTilesAt(executer.Position).Select(tile => floorManager.GetUnit(tile.Position)).Where(unit => unit != null))
            DamageToExecuter(unit);
        noticeGroup.Add("大きな地雷が炸裂した", Color.red);
        await UniTask.Yield(cancellationToken: token);
    }
}
