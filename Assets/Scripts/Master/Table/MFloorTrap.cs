using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MFloorTrap")]
public class MFloorTrap : ScriptableObject
{
    [SerializeField]
    private List<FloorTrapInfo> data;

    public List<FloorTrapInfo> All => data;
    private Dictionary<int, List<FloorTrapInfo>> groups = new();

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

    public List<FloorTrapInfo> GetByGroupId(int groupId)
        => groups.TryGetValue(groupId, out var result) ? result : new();

    public FloorTrapInfo LotteryTrap(int groupId)
    {
        var candidate = GetByGroupId(groupId);
        if (candidate.Count <= 1) return candidate.FirstOrDefault();

        return Lottery.Get(candidate);
    }
}
