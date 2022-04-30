using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DungeonGenerator
{
    public static bool ExisitDungeonData(DungeonSetting setting)
    {
        var dirInfo = new DirectoryInfo(System.IO.Path.Combine(FloorUtil.SavePath, setting.name));
        return dirInfo.Exists;
    }

    public static void Generate(DungeonSetting setting, bool reset = true)
    {
        if (reset)
            FloorUtil.CleanupDungeonFolder(setting.name);
        FloorUtil.CreateFolder(setting.name);

        var floorIndex = 1;
        foreach(var floorSetting in setting.FloorList)
        {
            var floorCount = 0;
            while (floorCount < floorSetting.SameSettingCount)
            {
                FloorUtil.Serialize(setting.name, GenerateFloor(floorSetting.Size.x, floorSetting.Size.y, floorSetting.MaxRoomCount), floorIndex);
                floorCount++;
                floorIndex++;
            }
        }
    }

    public static FloorData GenerateFloor(int mapWidth = 20, int mapHeight = 20, int maxRoom = 3)
    {
        Area.Count = 0;
        Area.MaxRoomNum = maxRoom;
        var rootArea = new Area(0, 0, mapWidth, mapHeight);
        rootArea.Split();

        Debug.Log("area count:" + Area.Count);

        var result = new List<Room>();
        rootArea.RecursiveCrateRoom();
        rootArea.RecursivePrintStatus();
        rootArea.RecursiveGetRoom(ref result);

        var areaList = new List<Area>();
        rootArea.RecursiveGetArea(ref areaList);
        foreach (var area in areaList)
            area.CreateAdjacentList(areaList);

        var pathList = new List<Path>();
        rootArea.RecursiveCreatePath(ref pathList);
        var map = new TileData[mapWidth, mapHeight];
        for (var x = 0; x < mapWidth; x++)
        {
            for (var y = 0; y < mapHeight; y++)
            {
                map[x, y] = new TileData()
                {
                    Position = new Vector2Int(x, y),
                    Type = TileType.Wall
                };
            }
        }
        foreach (var room in result)
        {
            var x = room.X;
            var y = room.Y;
            var width = room.Width;
            var height = room.Height;
            for (var row = y; row < y + height; row++)
            {
                for (var column = x; column < x + width; column++)
                {
                    map[row, column].Position = new Vector2Int(row, column);
                    map[row, column].Type = TileType.Room;
                    map[row, column].Id = room.AreaId;
                }
            }
        }

        foreach (var path in pathList)
        {
            foreach(var position in path.PathPositonList)
            {
                map[position.y, position.x].Position = position;
                map[position.y, position.x].Type = TileType.Path;
            }
        }
        return new FloorData(map);
    }
}
