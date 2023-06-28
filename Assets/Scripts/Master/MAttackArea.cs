using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MAttackArea")]
public class MAttackArea : TableBase<AttackAreaData>
{
    public new string TableName => "m_attack_area";
}
