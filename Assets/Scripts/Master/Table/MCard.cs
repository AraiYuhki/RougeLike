using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MCard")]
public class MCard : ScriptableObject
{
    [SerializeField]
    private List<CardInfo> data;

    private Dictionary<int, CardInfo> dictionary = new();
    private Dictionary<CardType, List<CardInfo>> typeMap = new();

    public List<CardInfo> All => data;

    public CardInfo GetById(int id)
    {
        if (dictionary.TryGetValue(id, out var result)) return result;
        return null;
    }

    public List<CardInfo> GetByType(CardType type)
    {
        if (typeMap.TryGetValue(type, out var result)) return result;
        return null;
    }

    public void OnEnable()
    {
        dictionary = data.ToDictionary(data => data.Id, data => data);
        typeMap.Clear();
        foreach (var row in data)
        {
            if (!typeMap.ContainsKey(row.Type))
                typeMap.Add(row.Type, new());
            typeMap[row.Type].Add(row);
        }
    }
}
