using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class DungeonInfo
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("name")]
    private string name;
    [SerializeField, CsvColumn("isTower")]
    private bool isTower;

    public int Id => id;
    public string Name => name;
    public bool IsTower => isTower;

    public DungeonInfo() { }

    public FloorInfo GetFloor(int floorNum)
    {
        var floorList = DB.Instance.MFloor.GetByDungeonId(id);
        var count = 0;
        foreach (var floor in floorList)
        {
            count += floor.SameSettingCount;
            if (count >= floorNum - 1) return floor;
        }
        return floorList.Last();
    }

    public DungeonInfo Clone()
    {
        return new DungeonInfo()
        {
            id = id,
            name = name,
            isTower = isTower
        };
    }
#if UNITY_EDITOR
    public DungeonInfo(int id) => this.id = id;
    public void SetName(string name) => this.name = name;
    public void SetIsTower(bool isTower) => this.isTower = isTower;

    public void Apply(DungeonInfo data)
    {
        name = data.name;
        isTower = data.isTower;
    }
#endif
}
