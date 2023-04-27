using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

    public static FloorData GenerateFloor(int mapWidth = 20, int mapHeight = 20, int maxRoom = 3, float deletePathPercent = 10f)
    {
        Area.Count = 0;
        Area.MaxRoomNum = maxRoom;
        var rootArea = new Area(0, 0, mapWidth, mapHeight);
        rootArea.Split();

        Debug.Log("area count:" + Area.Count);

        var roomList = new List<Room>();
        rootArea.RecursiveCrateRoom();
        rootArea.RecursivePrintStatus();
        rootArea.RecursiveGetRoom(ref roomList);

        var areaList = new List<Area>();
        rootArea.RecursiveGetArea(ref areaList);
        foreach (var area in areaList)
            area.CreateAdjacentList(areaList);

        var pathList = new List<Path>();
        rootArea.RecursiveCreatePath(ref pathList);

        var data = new FloorData(mapWidth, mapHeight, roomList, pathList);
        data.DeletePath(deletePathPercent);
        return data;
    }
}
