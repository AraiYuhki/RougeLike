using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Room
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int AreaId { get; private set; }

    public Dictionary<int, Path> Connected { get; private set; } = new Dictionary<int, Path>();

    public List<int> ConnectedRooms => Connected.Keys.ToList();
    public List<Path> ConnectedPathList => Connected.Values.ToList();

    public int EndX => X + Width;

    public int EndY => Y + Height;

    public Room(int areaId, int x, int y, int width, int height)
    {
        AreaId = areaId;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public void AddPath(int toRoomID, Path path)
    {
        if (ConnectedRooms.Contains(toRoomID)) return;
        ConnectedRooms.Add(toRoomID);
        ConnectedPathList.Add(path);
        Connected.Add(toRoomID, path);
    }

    public bool CheckPathBeing(int toRoomId) => ConnectedRooms.Contains(toRoomId);
}

