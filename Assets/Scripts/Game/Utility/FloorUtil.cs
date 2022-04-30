using System;
using System.IO;
using UnityEngine;

public static class FloorUtil
{
    public static string SavePath => Application.dataPath.Replace("Assets", "Generated");

    private static string GetFloorFilePath(string dungeonName, int floorNum) => System.IO.Path.Combine(SavePath, dungeonName, $"{floorNum}F.flr");

    public static void CleanupDungeonFolder(string dungeonName)
    {
        var directoryInfo = new DirectoryInfo(System.IO.Path.Combine(SavePath, dungeonName));
        if (!directoryInfo.Exists) return;
        foreach (var file in directoryInfo.GetFiles())
            file.Delete();
    }

    public static void CreateFolder(string dungeonName)
    {
        var directoryInfo = new DirectoryInfo(System.IO.Path.Combine(SavePath, dungeonName));
        if (directoryInfo.Exists) return;
        directoryInfo.Create();
    }

    public static bool Serialize(string dungeonName, FloorData data, int floorNum)
    {
        var filePath = GetFloorFilePath(dungeonName, floorNum);
        try
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(data.Size.x);
                bw.Write(data.Size.y);
                bw.Write(data.SpawnPoint.x);
                bw.Write(data.SpawnPoint.y);
                bw.Write(data.StairPosition.x);
                bw.Write(data.StairPosition.y);
                for (var y = 0; y < data.Size.y; y++)
                {
                    for (var x = 0; x < data.Size.x; x++)
                    {
                        bw.Write((int)data.Map[x, y].Type);
                        bw.Write(data.Map[x, y].Id);
                    }
                }
            }
        } 
        catch(Exception e)
        {
            Debug.LogException(e);
            return false;
        }
        return true;
    }

    public static FloorData Deserialize(string filePath)
    {
        var data = new FloorData();
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
            var size = new Vector2Int(br.ReadInt32(), br.ReadInt32());
            var spawnPoint = new Vector2Int(br.ReadInt32(), br.ReadInt32());
            var stairPoint = new Vector2Int(br.ReadInt32(), br.ReadInt32());
            data.SpawnPoint = spawnPoint;
            data.StairPosition = stairPoint;
            var map = new TileData[size.x, size.y];
            for (var y = 0; y < size.y; y++)
            {
                for (var x = 0; x < size.x; x++)
                {
                    map[x, y] = new TileData()
                    {
                        Type = (TileType)br.ReadInt32(),
                        Id = br.ReadInt32(),
                        Position = new Vector2Int(x, y)
                    };
                }
            }
            return new FloorData()
            {
                SpawnPoint = spawnPoint,
                StairPosition = stairPoint,
                Map = map
            };
        }
    }

    public static FloorData Deserialize(string dungeonName, int floorNum)
    {
        var filePath = GetFloorFilePath(dungeonName, floorNum);
        return Deserialize(filePath);
    }
}
