using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MWeapon")]
public class MWeapon : TableBase<WeaponData>
{
    public new string TableName => "m_weapon";
}
