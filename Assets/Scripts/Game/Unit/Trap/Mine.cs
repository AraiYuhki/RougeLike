using Cysharp.Threading.Tasks;
using UnityEngine;

public class Mine : Trap
{
    [SerializeField]
    private int damage = 10;
    public override TrapType Type => TrapType.Mine;

    public override async UniTask Execute(Unit executer)
    {
        await base.Execute(executer);
        executer.Damage(damage);
        noticeGroup.Add("地雷が炸裂した", Color.red);
    }
}
