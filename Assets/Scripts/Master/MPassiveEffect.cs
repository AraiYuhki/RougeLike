using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MPassiveEffect")]
public class MPassiveEffect : TableBase<PassiveEffectData>
{
    public new string TableName => "m_passive_effect";
}
