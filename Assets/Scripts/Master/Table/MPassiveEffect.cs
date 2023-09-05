using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MPassiveEffect")]
public class MPassiveEffect : ScriptableObject
{
    [SerializeField]
    private List<PassiveEffectInfo> data;

    public List<PassiveEffectInfo> All => data;

    private Dictionary<int, PassiveEffectInfo> groups = new();
    private Dictionary<PassiveEffectType, List<PassiveEffectInfo>> typeMap = new();

    public PassiveEffectInfo GetById(int id)
    {
        if (groups.TryGetValue(id, out var result)) return result;
        return null;
    }

    public List<PassiveEffectInfo> GetByType(PassiveEffectType type)
    {
        if (typeMap.TryGetValue(type, out var result)) return result;
        return null;
    }

    public void OnEnable()
    {
        groups = data.ToDictionary(row => row.Id, row => row);
        typeMap.Clear();
        foreach (var row in data)
        {
            if (!typeMap.ContainsKey(row.EffectType))
                typeMap.Add(row.EffectType, new());
            typeMap[row.EffectType].Add(row);
        }
    }

}
