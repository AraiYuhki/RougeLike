using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class DungeonSetting : ScriptableObject
{
    [SerializeField]
    private List<FloorSetting> floorList = new List<FloorSetting>();
    [SerializeField]
    private bool isTower = false;

    public List<FloorSetting> FloorList => floorList;
    public bool IsTower => isTower;

    public FloorSetting GetFloor(int floorNum)
    {
        var count = 0;
        foreach(var floor in floorList)
        {
            count += floor.SameSettingCount;
            if (count >= floorNum - 1) return floor;
        }
        return floorList.Last();
    }
}
