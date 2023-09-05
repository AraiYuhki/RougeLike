using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MFloorEnemySpawn")]
public class MFloorEnemySpawn : ScriptableObject
{
    [SerializeField]
    private List<FloorEnemySpawnInfo> data;

    private Dictionary<int, FloorEnemySpawnInfo> dictionary = new();
    private Dictionary<int, List<FloorEnemySpawnInfo>> groups = new();

    public List<FloorEnemySpawnInfo> All => data;

    public FloorEnemySpawnInfo GetById(int id)
    {
        if (dictionary.TryGetValue(id, out var result)) return result;
        return null;
    }

    public List<FloorEnemySpawnInfo> GetByGroupId(int groupId)
    {
        if (groups.TryGetValue(groupId, out var result)) return result;
        return null;
    }

    public void OnEnable()
    {
        dictionary = data.ToDictionary(data => data.Id, data => data);
        groups.Clear();
        foreach (var row in data)
        {
            if (!groups.ContainsKey(row.GroupId))
                groups.Add(row.GroupId, new());
            groups[row.Id].Add(row);
        }
    }
}
