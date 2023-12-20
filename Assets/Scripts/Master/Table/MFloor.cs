using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MFloor")]
public class MFloor : ScriptableObject
{
    [SerializeField]
    private List<FloorInfo> data;

    private Dictionary<int, List<FloorInfo>> groups = new();
    public List<FloorInfo> All => data;

    public List<FloorInfo> GetByDungeonId(int dungeonId)
    {
        if (groups.TryGetValue(dungeonId, out var result)) return result;
        return new List<FloorInfo>();
    }

    public void OnEnable()
    {
        groups.Clear();
        foreach (var row in data)
        {
            if (!groups.ContainsKey(row.DungeonId))
                groups.Add(row.DungeonId, new());
            groups[row.DungeonId].Add(row);
        }
    }

#if UNITY_EDITOR
    public void RemoveByDungeonId(int dungeonId)
    {
        data = data.Where(info => info.DungeonId != dungeonId).ToList();
        OnEnable();
    }
    public void AddData(List<FloorInfo> data)
    {
        this.data.AddRange(data);
        OnEnable();
    }
#endif
}
