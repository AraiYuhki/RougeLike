using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MShield")]
public class MShield : TableBase<ShieldData>
{
    public new static string TableName => "m_shield";
}
