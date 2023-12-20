using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Trap
{
    [SerializeField]
    private int damage = 10;
    public override TrapType Type => TrapType.Mine;

    public override void Execute(Unit executer)
    {
        base.Execute(executer);
        executer.Damage(damage);
        noticeGroup.Add("地雷が炸裂した", Color.red);
    }
}
