﻿using System.Collections.Generic;
using UnityEngine;

class Room
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int AreaId { get; private set; }
    public int ConnectX { get; private set; }
    public int ConnectY { get; private set; }

    private Dictionary<int, Path> pathList = new Dictionary<int, Path>();

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

    public void SetConnectPoint(int x, int y)
    {
        ConnectX = x;
        ConnectY = y;
    }

    public void AddPath(int toRoomID, Path path)
    {
        if (pathList.ContainsKey(toRoomID))
        {
            return;
        }
        pathList.Add(toRoomID, path);
    }

    public bool CheckPathBeing(int toRoomId)
    {
        return pathList.ContainsKey(toRoomId);
    }

    public void ScaleDown()
    {
        int direction = Random.Range(0, 2);
        if (direction == 0 && Width > 1)
        {
            if (ConnectX - X > EndX - ConnectX)
            {
                X++;
                Width--;
            }
            else if (EndX - ConnectX > 1)
            {
                Width--;
            }
        }
        else if (Height > 1)
        {
            if (ConnectY - Y > EndY - ConnectY)
            {
                Y++;
                Height--;
            }
            else if (EndY - ConnectY > 1)
            {
                Height--;
            }
        }
    }
}

