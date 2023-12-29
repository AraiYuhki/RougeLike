﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private Item[] gemTemplates = new Item[0];

    public List<ItemData> ItemList { get; private set; } = new();


    public void Initialize(int sumPrice, int minCount = 1, int maxCount = 5)
    {
        var max = sumPrice / minCount;
        var min = sumPrice / maxCount;
        var spawnedGems = 0;
        while(spawnedGems < sumPrice)
        {
            var value = Random.Range(min, max);
            spawnedGems += value;
            Spawn(value, GetSpawnTile());
        }
    }

    public void Clear()
    {
        foreach (var item in ItemList)
            Destroy(item.gameObject);
        ItemList.Clear();
    }

    public Item Drop(int price, Vector2Int position, bool isAnimation = false)
    {
        var item = Instantiate(gemTemplates.First(), floorManager.transform);
        var itemData = new ItemData(item, position, price);
        if (isAnimation)
        {
            item.JumpTo();
        }
        ItemList.Add(itemData);
        floorManager.SetItem(itemData, position);
        return item;
    }

    private void Spawn(int price, TileData tile)
    {
        var template = gemTemplates.First();
        var item = Instantiate(template, floorManager.transform);
        var itemData = new ItemData(item, tile.Position, price);
        floorManager.SetItem(itemData, tile.Position);
        ItemList.Add(itemData);
    }

    public void Despawn(ItemData item)
    {
        ItemList.Remove(item);
        Destroy(item.gameObject);
    }

    private TileData GetSpawnTile()
    {
        var playerTile = floorManager.GetTile(player.Position);
        return floorManager.GetRoomTiles().Where(tile => Filter(tile, playerTile.Position)).Random();
    }

    private bool Filter(TileData tile, Point playerPosition)
    {
        if (floorManager.GetItem(tile.Position) != null) return false;
        if (floorManager.GetUnit(tile.Position) != null) return false;
        if (tile.Position == playerPosition) return false;
        return true;
    }
}
