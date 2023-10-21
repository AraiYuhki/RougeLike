using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MFloorShop")]
public class MFloorShop : ScriptableObject
{
    [SerializeField, CsvColumn("id")]
    private List<FloorShopInfo> data = new();

    public List<FloorShopInfo> All => data;

    public Dictionary<int, FloorShopInfo> dictionary = new();

    public void OnEnable()
    {
        dictionary = data.ToDictionary(info => info.Id, info => info);
    }

    public FloorShopInfo GetById(int id)
    {
        if (dictionary.TryGetValue(id, out var result)) return result;
        return null;
    }
}
