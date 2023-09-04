using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MAttack")]
public class MAttack : ScriptableObject
{
    [SerializeField]
    private List<AttackInfo> data;

    private Dictionary<int, List<AttackInfo>> groups = new Dictionary<int, List<AttackInfo>>();
    public List<AttackInfo> All => data;
    public List<AttackInfo> GetByGroupId(int groupId)
    {
        if (groups.TryGetValue(groupId, out var result)) return result;
        return null;
    }

    public void OnEnable()
    {
        foreach (var row in data)
        {
            if (!groups.ContainsKey(row.GroupId))
                groups.Add(row.GroupId, new());
            groups[row.GroupId].Add(row);
        }
    }
}
