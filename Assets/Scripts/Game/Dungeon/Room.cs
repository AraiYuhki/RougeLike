using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Room
{
    [SerializeField]
    private Vector2Int position;
    [SerializeField]
    private Vector2Int size;
    [SerializeField]
    private int id;

    public int X => position.x;
    public int Y => position.y;
    public int Width => size.x;
    public int Height => size.y;
    public int Id => id;

    /// <summary>
    /// 接続先番号
    /// </summary>
    public Dictionary<int, Path> Connected { get; private set; } = new Dictionary<int, Path>();
    /// <summary>
    /// (接続先の部屋ID, 接続元の部屋から通路への入り口の座標)
    /// </summary>
    public Dictionary<int, Point> ConnectedPoint { get; private set; } = new Dictionary<int, Point>();
    public List<int> ConnectedRooms => Connected.Keys.ToList();
    public List<Path> ConnectedPathList => Connected.Values.ToList();

    public int EndX => X + Width;

    public int EndY => Y + Height;

    public Point Center => new Point(X + Width * 0.5f, Y + Height * 0.5f);

    public Room()
    {
    }

    public Room(int id, int x, int y, int width, int height)
    {
        this.id = id;
        position = new Vector2Int(x, y);
        size = new Vector2Int(width, height);
    }

    /// <summary>
    /// 部屋に接続されている通路を追加する
    /// </summary>
    /// <param name="toRoomID">接続先の部屋ID</param>
    /// <param name="path">通路インスタンス　</param>
    /// <param name="connectedPoint">接続元の部屋から通路に入る座標</param>
    public void AddPath(int toRoomID, Path path, Point connectedPoint)
    {
        if (Connected.ContainsKey(toRoomID)) return;
        Connected.Add(toRoomID, path);
        ConnectedPoint.Add(toRoomID, connectedPoint);
    }

    public void RemovePath(int toRoomId)
    {
        if (!Connected.ContainsKey(toRoomId)) return;
        Connected.Remove(toRoomId);
        ConnectedPoint.Remove(toRoomId);
    }

    public bool CheckPathBeing(int toRoomId) => ConnectedRooms.Contains(toRoomId);
}

