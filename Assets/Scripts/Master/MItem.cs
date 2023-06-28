using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Master/MItem")]
public class MItem : TableBase<UsableItemData>
{
    public new string TableName => "m_item";
}
