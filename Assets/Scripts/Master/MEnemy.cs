using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Master/MEnemy")]
public class MEnemy : TableBase<EnemyData>
{
    public new string TableName => "m_enemy";
}
