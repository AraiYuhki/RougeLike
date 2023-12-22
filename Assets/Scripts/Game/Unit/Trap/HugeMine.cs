using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class HugeMine : Trap
{
    [SerializeField]
    private int damage = 10;
    public override TrapType Type => TrapType.HugeMine;

    public override async UniTask Execute(Unit executer)
    {
        await base.Execute(executer);
        executer.Damage(damage);
        foreach (var unit in floorManager.GetAroundTilesAt(executer.Position).Select(tile => floorManager.GetUnit(tile.Position)).Where(unit => unit != null))
            unit.Damage(damage);
        noticeGroup.Add("大きな地雷が炸裂した", Color.red);
    }
}
