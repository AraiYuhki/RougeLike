using System.Collections.Generic;

public class DungeonGenerator
{
    public static FloorData GenerateFloor(int mapWidth = 20, int mapHeight = 20, int maxRoom = 3, float deletePathPercent = 1f)
    {
        Area.Count = 0;
        Area.MaxRoomNum = maxRoom;
        var rootArea = new Area(0, 0, mapWidth, mapHeight);
        rootArea.Split();

        var roomList = new List<Room>();
        rootArea.RecursiveCrateRoom();
        rootArea.RecursivePrintStatus();
        rootArea.RecursiveGetRoom(ref roomList);

        var areaList = new List<Area>();
        rootArea.RecursiveGetArea(ref areaList);
        foreach (var area in areaList)
            area.CreateAdjacentList(areaList);

        var pathList = new List<Path>();
        var pathIndex = 1;
        rootArea.RecursiveCreatePath(ref pathList, ref pathIndex);

        var data = new FloorData(mapWidth, mapHeight, roomList, pathList);
        data.DeletePath(deletePathPercent);
        return data;
    }
}
