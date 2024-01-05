using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Room
{
    [SerializeField]
    private int id;
    [SerializeField]
    private RectInt rect;
    [SerializeField]
    private Encyclopedia<int, Path> connectedPaths = new();
    [SerializeField]
    private Encyclopedia<int, Point> connectedPoints = new();

    public int X => rect.x;
    public int Y => rect.y;
    public int Width => rect.width;
    public int Height => rect.height;
    public int Id => id;

    /// <summary>
    /// 接続先番号
    /// </summary>
    public Dictionary<int, Path> ConnectedPaths => connectedPaths;
    /// <summary>
    /// (接続先の部屋ID, 接続元の部屋から通路への入り口の座標)
    /// </summary>
    public Dictionary<int, Point> ConnectedPoints => connectedPoints;
    public List<int> ConnectedRooms => connectedPaths.Keys.ToList();
    public List<Path> ConnectedPathList => connectedPaths.Values.ToList();

    public int EndX => rect.max.x;

    public int EndY => rect.max.y;

    public Point Center => new Point(rect.center.x, rect.center.y);

    public Room()
    {
    }

    public Room(int id, int x, int y, int width, int height)
    {
        this.id = id;
        rect = new RectInt(x, y, width, height);
    }

    /// <summary>
    /// 部屋に接続されている通路を追加する
    /// </summary>
    /// <param name="toRoomID">接続先の部屋ID</param>
    /// <param name="path">通路インスタンス　</param>
    /// <param name="connectedPoint">接続元の部屋から通路に入る座標</param>
    public void AddPath(int toRoomID, Path path, Point connectedPoint)
    {
        if (connectedPaths.ContainsKey(toRoomID)) return;
        connectedPaths.Add(toRoomID, path);
        connectedPoints.Add(toRoomID, connectedPoint);
    }

    public void RemovePath(int toRoomId)
    {
        if (!connectedPaths.ContainsKey(toRoomId)) return;
        connectedPaths.Remove(toRoomId);
        connectedPoints.Remove(toRoomId);
    }

    public bool CheckPathBeing(int toRoomId) => ConnectedRooms.Contains(toRoomId);
}

