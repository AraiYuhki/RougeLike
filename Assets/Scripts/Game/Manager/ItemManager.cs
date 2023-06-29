using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private Item[] gemTemplates = new Item[0];
    [SerializeField]
    private Item[] itemTemplates = new Item[0];
    [SerializeField]
    private Item[] weaponTemplates = new Item[0];
    [SerializeField]
    private Item[] shieldTemplates = new Item[0];

    public List<Item> ItemList { get; private set; } = new List<Item>();
    private FloorManager floorManager => ServiceLocator.Instance.FloorManager;

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

    public void Initialize(int count = 5)
    {
        foreach(var _ in Enumerable.Range(0, 5))
        {
            var type = UnityEngine.Random.Range(0, 4);
            switch(type)
            {
                case 0:
                    Spawn(DataBase.Instance.GetTable<MItem>().Data.Random().Clone() as UsableItemData, 0);
                    break;
                case 1:
                    Spawn(DataBase.Instance.GetTable<MWeapon>().Data.Random().Clone() as WeaponData, 0);
                    break;
                case 2:
                    Spawn(DataBase.Instance.GetTable<MShield>().Data.Random().Clone() as ShieldData, 0);
                    break;
                default:
                    Spawn(UnityEngine.Random.Range(100, 3000));
                    break;
            }
        }
    }

    public Item Drop(ItemBase data, int index, Vector2Int position, bool isAnimation = false)
    {
        var template = GetTemplate(data, index);
        var item = Instantiate(template, floorManager.transform);
        item.Data = data;
        item.SetPosition(floorManager.GetTile(position.x, position.y));
        if (isAnimation)
            item.transform.DOLocalJump(item.transform.localPosition, 5, 1, 0.4f);
        floorManager.SetItem(item, position);
        return item;
    }

    public Item Drop(int price, Vector2Int position, bool isAnimation = false)
    {
        var item = Instantiate(gemTemplates.First(), floorManager.transform);
        item.GemCount = price;
        item.SetPosition(floorManager.GetTile(position.x, position.y));
        if (isAnimation)
            item.transform.DOLocalJump(item.transform.localPosition, 1, 1, 0.4f);
        ItemList.Add(item);
        floorManager.SetItem(item, position);
        return item;
    }

    public void Spawn(ItemBase data, int index, bool isAnimation = false)
    {
        var template = GetTemplate(data, index);
        var item = Instantiate(template, floorManager.transform);
        item.Data = data;
        var tile = floorManager.GetRoomTiles().Where(tile => floorManager.GetItem(tile.Position) == null).Random();
        item.SetPosition(tile);
        if (isAnimation)
            item.transform.DOLocalJump(item.transform.localPosition, 5, 1, 0.4f);
        floorManager.SetItem(item, tile.Position);
        ItemList.Add(item);
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

    private Item GetTemplate(ItemBase item, int index)
    {
        if (item is UsableItemData) return itemTemplates[index];
        if (item is WeaponData) return weaponTemplates[index];
        if (item is ShieldData) return shieldTemplates[index];
        throw new InvalidOperationException($"{item.GetType()} is not supported");
    }
}
