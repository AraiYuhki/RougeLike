using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DungeonSetting : ScriptableObject
{
    [SerializeField]
    private List<FloorSetting> floorList = new List<FloorSetting>();

    public List<FloorSetting> FloorList => floorList;
}
