using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MAttackArea")]
public class MAttackArea : ScriptableObject
{
    [SerializeField]
    private List<AttackAreaInfo> data;

    private Dictionary<int, AttackAreaInfo> dictionary = new();

    public List<AttackAreaInfo> All => data;
    public void OnEnable()
    {
        dictionary = data.ToDictionary(row => row.Id, row => row);
    }

    public AttackAreaInfo GetById(int id)
    {
        if (dictionary.TryGetValue(id, out var result)) return result;
        return null;
    }
#if UNITY_EDITOR
    public void Reset()
    {
        OnEnable();
    }
#endif
}
