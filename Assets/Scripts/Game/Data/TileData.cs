using System;
using UnityEngine;

[Serializable]
public class TileData
{
    /// <summary>
    /// À•W
    /// </summary>
    public Point Position { get; set; }
    /// <summary>
    /// •”‰®‚©H
    /// </summary>
    public TileType Type { get; set; } = TileType.Wall;
    /// <summary>
    /// •”‰®‚©’Ê˜H‚ÌID
    /// </summary>
    public int Id { get; set; } = -1;

    public bool IsWall => Type == TileType.Wall;
    public bool IsRoom => Type == TileType.Room;
}
