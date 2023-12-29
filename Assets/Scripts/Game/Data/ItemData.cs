using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData
{
    [SerializeField]
    private Vector2Int position;
    [SerializeField]
    private int gemCount = 0;

    private Item owner;

    public Vector2Int Position => position;
    public int GemCount => gemCount;
    public Item Owner => owner;
    public GameObject gameObject => owner.gameObject;

    public ItemData() { }
    public ItemData(Item owner, Vector2Int position, int gemCount)
    {
        this.owner = owner;
        this.position = position;
        this.gemCount = gemCount;
        owner.transform.localPosition = new Vector3(position.x, 0f, position.y);
    }

    public void SetPosition(TileData tile)
    {
        position = tile.Position;
        owner.transform.localPosition = new Vector3(tile.Position.X, 0f, tile.Position.Y);
    }
}
