using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private Item[] gemTemplates = new Item[0];

    public List<Item> ItemList { get; private set; } = new List<Item>();


    public void Initialize(int sumPrice, int minCount = 1, int maxCount = 5)
    {
        var max = sumPrice / minCount;
        var min = sumPrice / maxCount;
        var spawnedGems = 0;
        while(spawnedGems < sumPrice)
        {
            var value = UnityEngine.Random.Range(min, max);
            spawnedGems += value;
            Spawn(value);
        }
    }

    public void Clear()
    {
        foreach (var item in ItemList)
            Destroy(item);
        ItemList.Clear();
    }

    public Item Drop(int price, Vector2Int position, bool isAnimation = false)
    {
        var item = Instantiate(gemTemplates.First(), floorManager.transform);
        item.GemCount = price;
        item.SetPosition(floorManager.GetTile(position.x, position.y));
        if (isAnimation)
        {
            item.JumpTo();
        }
        ItemList.Add(item);
        floorManager.SetItem(item, position);
        return item;
    }

    public void Spawn(int price)
    {
        var template = gemTemplates.First();
        var item = Instantiate(template, floorManager.transform);
        item.GemCount = price;
        var tile = floorManager.GetRoomTiles().Where(tile => floorManager.GetItem(tile.Position) == null).Random();
        item.SetPosition(tile);
        floorManager.SetItem(item, tile.Position);
        ItemList.Add(item);
    }

    public void Despawn(Item item)
    {
        ItemList.Remove(item);
        Destroy(item.gameObject);
    }
}
