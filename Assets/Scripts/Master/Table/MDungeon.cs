using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MDungeon")]
public class MDungeon : ScriptableObject
{
    [SerializeField]
    private List<DungeonInfo> data;

    private Dictionary<int, DungeonInfo> groups = new();

    public List<DungeonInfo> All => data;

    public DungeonInfo GetById(int id)
    {
        if (groups.TryGetValue(id, out var result)) return result;
        return null;
    }

    public void OnEnable()
    {
        groups = data.ToDictionary(row => row.Id, row => row);
    }

#if UNITY_EDITOR
    public void RemoveById(int id)
    {
        data.Remove(data.FirstOrDefault(row => row.Id == id));
        OnEnable();
    }
#endif
}
