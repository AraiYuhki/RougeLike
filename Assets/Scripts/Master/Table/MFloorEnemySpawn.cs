using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MFloorEnemySpawn")]
public class MFloorEnemySpawn : ScriptableObject
{
    [SerializeField]
    private List<FloorEnemySpawnInfo> data;

    private Dictionary<int, List<FloorEnemySpawnInfo>> groups = new();

    public List<FloorEnemySpawnInfo> All => data;

    public List<FloorEnemySpawnInfo> GetByGroupId(int groupId)
    {
        if (groups.TryGetValue(groupId, out var result)) return result;
        return null;
    }

    public void OnEnable()
    {
        groups.Clear();
        foreach (var row in data)
        {
            if (!groups.ContainsKey(row.GroupId))
                groups.Add(row.GroupId, new());
            groups[row.GroupId].Add(row);
        }
    }
#if UNITY_EDITOR
    public void RemoveFromGroupId(int groupId)
    {
        data = data.Where(data => data.GroupId != groupId).ToList();
        OnEnable();
    }
    public void AddData(List<FloorEnemySpawnInfo> data)
    {
        this.data.AddRange(data);
        OnEnable();
    }
#endif
}
