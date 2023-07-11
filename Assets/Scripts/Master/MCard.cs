using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MCard")]
public class MCard : TableBase<CardData>
{
    public new string TableName => "m_card";
}
