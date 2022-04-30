using System.Collections.Generic;
using UnityEngine;

class Area
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
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
        Id = Count;
        Count++;
    }

    public void Split()
    {
        if (Width < AreaSizeMin && Height < AreaSizeMin) return;
        if (Count > MaxRoomNum) return;

        var horizontal = (Random.Range(0, 2) == 1 && Height >= AreaSizeMin * 2);
        var dividePoint = AreaSizeMin;
        if (horizontal)
        {
            if (Width < AreaSizeMin * 2) return;
            dividePoint = Random.Range(AreaSizeMin, Width - AreaSizeMin);
            Debug.Log("width:" + Width + " dividePoint:" + dividePoint);
            child[0] = new Area(X, Y, dividePoint, Height);
            child[1] = new Area(X + dividePoint, Y, Width - dividePoint, Height);
        }
        else
        {
            if (Height < AreaSizeMin * 2) return;
            dividePoint = Random.Range(AreaSizeMin, Height - AreaSizeMin);
            child[0] = new Area(X, Y, Width, dividePoint);
            child[1] = new Area(X, Y + dividePoint, Width, Height - dividePoint);
        }
        child[0].Split();
        child[1].Split();
    }

    public void RecursivePrintStatus()
    {
        if (child[0] == null && child[1] == null)
        {
            Debug.Log(X + ":" + Y + ":" + Width + ":" + Height);
        }
        else
        {
            child[0].RecursivePrintStatus();
            child[1].RecursivePrintStatus();
        }
    }

    public void RecursiveGetArea(ref List<Area> result)
    {
        if (child[0] == null && child[1] == null)
        {
            result.Add(this);
            return;
        }
        child[0].RecursiveGetArea(ref result);
        child[1].RecursiveGetArea(ref result);
    }

    public List<Room> RecursiveGetRoom(ref List<Room> result)
    {
        if (Room != null)
        {
            result.Add(Room);
        }
        else
        {
            if (child[0] != null)
            {
                child[0].RecursiveGetRoom(ref result);
            }
            if (child[1] != null)
            {
                child[1].RecursiveGetRoom(ref result);
            }
        }
        return result;
    }

    public void RecursiveCrateRoom()
    {
        if (child[0] == null && child[1] == null)
        {
            var width = Mathf.Max(RoomSizeMin, Random.Range(RoomSizeMin, this.Width - 2));
            var height = Mathf.Max(RoomSizeMin, Random.Range(RoomSizeMin, this.Height - 2));
            var x = Random.Range(1, this.Width - width - 1) + this.X;
            var y = Random.Range(1, this.Height - height - 1) + this.Y;
            Room = new Room(Id, x, y, width, height);
        }
        else
        {
            if (child[0] != null)
            {
                child[0].RecursiveCrateRoom();
            }
            if (child[1] != null)
            {
                child[1].RecursiveCrateRoom();
            }
        }
    }

    public void RecursiveCreatePath(ref List<Path> pathList)
    {
        if (child[0] == null && child[1] == null)
        {
            for (var cnt = 0; cnt < adjacent.Count; cnt++)
            {
                var toId = adjacent[cnt].area.Id;
                if (Room.CheckPathBeing(toId))
                {
                    if (!adjacent[cnt].area.Room.CheckPathBeing(Id))
                    {
                        Debug.Log("エラー 片方の部屋にしか道が登録されていません！ fromArea:" + Id + " toArea:" + toId);
                    }
                    else
                    {
                        continue;
                    }
                }
                var fromRoom = Room;
                var toRoom = adjacent[cnt].area.Room;
                var path = new Path();
                var fromPosition = Vector2Int.zero;
                var toPosition = Vector2Int.zero;

                if (adjacent[cnt].isHorizontal)
                {
                    if (X > adjacent[cnt].area.X)
                    {
                        fromPosition.x = fromRoom.X;
                        toPosition.x = (toRoom.X + toRoom.Width);
                        path.Dir = Path.Direction.LEFT;
                    }
                    else
                    {
                        fromPosition.x = fromRoom.X + fromRoom.Width;
                        toPosition.x = toRoom.X;
                        path.Dir = Path.Direction.RIGHT;
                    }

                    fromPosition.y = Random.Range(fromRoom.Y, fromRoom.Y + fromRoom.Height);
                    toPosition.y = Random.Range(toRoom.Y, toRoom.Y + toRoom.Height);
                }
                else
                {
                    if (Y > adjacent[cnt].area.Y)
                    {
                        fromPosition.y = fromRoom.Y;
                        toPosition.y = toRoom.Y + toRoom.Height;
                        path.Dir = Path.Direction.UP;
                    }
                    else
                    {
                        fromPosition.y = fromRoom.Y + fromRoom.Height;
                        toPosition.y = toRoom.Y;
                        path.Dir = Path.Direction.DOWN;
                    }
                    fromPosition.x = Random.Range(fromRoom.X, fromRoom.X + fromRoom.Width);
                    toPosition.x = Random.Range(toRoom.X, toRoom.X + toRoom.Width);
                }
                path.From = fromPosition;
                path.To = toPosition;
                path.FromAreaId = Id;
                path.ToAreaId = toId;
                path.CreatePositionList(this);

                fromRoom.AddPath(toId, path);
                toRoom.AddPath(Id, path);
                pathList.Add(path);
                Debug.Log("fromRoom(" + fromRoom.X + "," + fromRoom.Y + ":" + fromRoom.Width + "," + fromRoom.Height + ")"
                                + " toRoom(" + toRoom.X + "," + toRoom.Y + ":" + toRoom.Width + "," + toRoom.Height + ")");
                Debug.Log("from(" + fromPosition.x + "," + fromPosition.y + ")" + " to(" + toPosition.x + "," + toPosition.y + ")");
            }
        }
        else
        {
            child[0].RecursiveCreatePath(ref pathList);
            child[1].RecursiveCreatePath(ref pathList);
        }
    }


    public void CreateAdjacentList(List<Area> list)
    {
        adjacent = new List<AdjacentData>();
        for (var cnt = 0; cnt < list.Count; cnt++)
        {
            if (list[cnt] == this)
            {
                continue;
            }
            var data = new AdjacentData();
            if (list[cnt].X + list[cnt].Width == X || X + Width == list[cnt].X)
            {
                if ((Y >= list[cnt].Y && Y <= list[cnt].Y + list[cnt].Height) || (list[cnt].Y >= Y && list[cnt].Y <= Y + Height))
                {
                    data.area = list[cnt];
                    data.isHorizontal = true;
                    adjacent.Add(data);
                    Debug.Log(Id + " adjaceet width " + list[cnt].Id);
                }
            }
            else if (list[cnt].Y + list[cnt].Height == Y || Y + Height == list[cnt].Y)
            {
                if ((X >= list[cnt].X && X <= list[cnt].X + list[cnt].Width) || (list[cnt].X >= X && list[cnt].X <= X + Width))
                {
                    data.area = list[cnt];
                    data.isHorizontal = false;
                    adjacent.Add(data);
                    Debug.Log(Id + " adjaceet width " + list[cnt].Id);
                }
            }
        }
    }
}
