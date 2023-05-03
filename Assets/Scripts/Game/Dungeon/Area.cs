using System.Collections.Generic;
using UnityEngine;

public class Area
{
    public struct AdjacentData
    {
        public Area area;
        public bool isHorizontal;
    }

    private const int RoomSizeMin = 5;
    private const int AreaSizeMin = RoomSizeMin + 2;

    public static int MaxRoomNum { get; set; } = 3;
    public static int Count { get; set; } = 0;
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    private Area[] child = new Area[2];
    public Room Room { get; private set; }
    public int Id { get; private set; }
    private List<AdjacentData> adjacent;

    public Area(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Id = Count;
        Count++;
    }

    public void Split()
    {
        if (Width < AreaSizeMin && Height < AreaSizeMin) return;
        if (Count > MaxRoomNum) return;

        var horizontal = Random.Range(0, 2) == 1 && Height >= AreaSizeMin * 2;
        if (horizontal)
        {
            if (Width < AreaSizeMin * 2) return;
            var dividePoint = Random.Range(AreaSizeMin, Width - AreaSizeMin);
            child[0] = new Area(X, Y, dividePoint, Height);
            child[1] = new Area(X + dividePoint, Y, Width - dividePoint, Height);
        }
        else
        {
            if (Height < AreaSizeMin * 2) return;
            var dividePoint = Random.Range(AreaSizeMin, Height - AreaSizeMin);
            child[0] = new Area(X, Y, Width, dividePoint);
            child[1] = new Area(X, Y + dividePoint, Width, Height - dividePoint);
        }
        child[0].Split();
        child[1].Split();
    }

    public void RecursivePrintStatus()
    {
        if (child[0] == null && child[1] == null) return;
        child[0]?.RecursivePrintStatus();
        child[1]?.RecursivePrintStatus();
    }

    public void RecursiveGetArea(ref List<Area> result)
    {
        if (child[0] == null && child[1] == null)
        {
            result.Add(this);
            return;
        }
        child[0]?.RecursiveGetArea(ref result);
        child[1]?.RecursiveGetArea(ref result);
    }

    public List<Room> RecursiveGetRoom(ref List<Room> result)
    {
        if (Room != null)
        {
            result.Add(Room);
        }
        else
        {
            child[0]?.RecursiveGetRoom(ref result);
            child[1]?.RecursiveGetRoom(ref result);
        }
        return result;
    }

    public void RecursiveCrateRoom()
    {
        if (child[0] == null && child[1] == null)
        {
            var width = Mathf.Max(RoomSizeMin, Random.Range(RoomSizeMin, Width - 2));
            var height = Mathf.Max(RoomSizeMin, Random.Range(RoomSizeMin, Height - 2));
            var x = Random.Range(1, Width - width - 1) + X;
            var y = Random.Range(1, Height - height - 1) + Y;
            Room = new Room(Id, x, y, width, height);
            return;
        }
        child[0]?.RecursiveCrateRoom();
        child[1]?.RecursiveCrateRoom();
    }

    public void RecursiveCreatePath(ref List<Path> pathList, ref int pathIndex)
    {
        if (child[0] != null || child[1] != null)
        {
            child[0]?.RecursiveCreatePath(ref pathList, ref pathIndex);
            child[1]?.RecursiveCreatePath(ref pathList, ref pathIndex);
            return;
        }

        for (var index = 0; index < adjacent.Count; index++)
        {
            var toId = adjacent[index].area.Id;
            if (Room.CheckPathBeing(toId))
            {
                if (!adjacent[index].area.Room.CheckPathBeing(Id))
                    Debug.Log($"エラー 片方の部屋にしか道が登録されていません！ fromArea:{Id} toArea:{toId}");
                else
                    continue;
            }
            var fromRoom = Room;
            var toRoom = adjacent[index].area.Room;
            var path = new Path() { Id = pathIndex };
            pathIndex++;
            var fromPosition = Vector2Int.zero;
            var toPosition = Vector2Int.zero;

            if (adjacent[index].isHorizontal)
            {
                if (X > adjacent[index].area.X)
                {
                    fromPosition.x = fromRoom.X;
                    toPosition.x = toRoom.X + toRoom.Width;
                    path.Dir = Path.Direction.Left;
                }
                else
                {
                    fromPosition.x = fromRoom.X + fromRoom.Width;
                    toPosition.x = toRoom.X;
                    path.Dir = Path.Direction.Right;
                }

                fromPosition.y = Random.Range(fromRoom.Y, fromRoom.Y + fromRoom.Height);
                toPosition.y = Random.Range(toRoom.Y, toRoom.Y + toRoom.Height);
            }
            else
            {
                if (Y > adjacent[index].area.Y)
                {
                    fromPosition.y = fromRoom.Y;
                    toPosition.y = toRoom.Y + toRoom.Height;
                    path.Dir = Path.Direction.Up;
                }
                else
                {
                    fromPosition.y = fromRoom.Y + fromRoom.Height;
                    toPosition.y = toRoom.Y;
                    path.Dir = Path.Direction.Down;
                }
                fromPosition.x = Random.Range(fromRoom.X, fromRoom.X + fromRoom.Width);
                toPosition.x = Random.Range(toRoom.X, toRoom.X + toRoom.Width);
            }
            path.From = fromPosition;
            path.To = toPosition;
            path.FromRoomId = Id;
            path.ToRoomId = toId;
            path.CreatePositionList(this);

            fromRoom.AddPath(toId, path, path.From);
            toRoom.AddPath(Id, path, path.To);
            pathList.Add(path);
        }
    }


    public void CreateAdjacentList(List<Area> list)
    {
        adjacent = new List<AdjacentData>();
        for (var index = 0; index < list.Count; index++)
        {
            if (list[index] == this) continue;
            var data = new AdjacentData();
            if (list[index].X + list[index].Width == X || X + Width == list[index].X)
            {
                if ((Y >= list[index].Y && Y <= list[index].Y + list[index].Height) || (list[index].Y >= Y && list[index].Y <= Y + Height))
                {
                    data.area = list[index];
                    data.isHorizontal = true;
                    adjacent.Add(data);
                }
            }
            else if (list[index].Y + list[index].Height == Y || Y + Height == list[index].Y)
            {
                if ((X >= list[index].X && X <= list[index].X + list[index].Width) || (list[index].X >= X && list[index].X <= X + Width))
                {
                    data.area = list[index];
                    data.isHorizontal = false;
                    adjacent.Add(data);
                }
            }
        }
    }
}
