using System;
using UnityEngine;

[Serializable]
public class TileData
{
    /// <summary>
    /// 座標
    /// </summary>
    public Point Position { get; set; }
    /// <summary>
    /// 部屋か？
    /// </summary>
    public TileType Type { get; set; } = TileType.Wall;
    /// <summary>
    /// 部屋か通路のID
    /// </summary>
    public int Id { get; set; } = -1;

    public bool IsWall => Type == TileType.Wall;
    public bool IsRoom => Type == TileType.Room;
}
