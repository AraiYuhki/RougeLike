using UnityEngine;

public class PoisonTrap : Trap
{
    [SerializeField]
    private int power = 1;
    [SerializeField]
    private int turn = 10;
    public override TrapType Type => TrapType.PoisonTrap;

    public override void Execute(Unit executer)
    {
        base.Execute(executer);
        if (executer is Player player)
        {
            player.Data.AddAilment(AilmentType.Poison, power, turn);
            noticeGroup.Add("�v���C���[�͓ŕH�𓥂�", Color.red);
        }
        else if (executer is Enemy enemy)
        {
            enemy.Data.AddAilment(AilmentType.Poison, power, turn);
            noticeGroup.Add($"{enemy.Name}�͓ŕH�𓥂�", Color.magenta);
        }
    }

}
