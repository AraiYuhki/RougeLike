using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

[Serializable]
public class AttackData
{
    [SerializeField]
    private Vector2Int offset;
    [SerializeField]
    private uint rate;

    public Vector2Int Offset
    {
        get => offset;
        set => offset = value;
    }

    public uint Rate
    {
        get => rate;
        set => rate = value;
    }
}

[Serializable]
public class AttackAreaData
{
    private enum Direction
    {
        Right,
        Down,
        Left,
        Up,
    }
    [SerializeField]
    private int maxSize;
    [SerializeField]
    private Vector2Int center;
    [SerializeField]
    private AttackData[,] data;

    private List<AttackData>[] stripedData;

    private static int maxRadius = 10;
    private static Dictionary<int, List<(int x, int y)>> offsetMap;

    public int MaxSize
    {
        get => maxSize;
        set => maxSize = value;
    }

    public Vector2Int Center
    {
        get => center;
        set => center = value;
    }

    public AttackData[,] Data
    {
        get => data;
        set => data = value;
    }

    public List<AttackData>[] StripedData
    {
        get => stripedData;
        set => stripedData = value;
    }

    public static void InitializeOffsetMap()
    {
        var halfRadius = Mathf.FloorToInt(maxRadius / 2);
        offsetMap = new Dictionary<int, List<(int x, int y)>>();
        foreach (var index in Enumerable.Range(1, halfRadius + 1))
            offsetMap[index] = new List<(int x, int y)>();
        for (var radius = 1; radius <= maxRadius; radius++)
        {
            var currentPosition = new Vector2Int(-radius, -radius);
            var offsetList = new List<(int x, int y)>(){ (currentPosition.x, currentPosition.y) };
            foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                for (var count = 0;count < radius * 2; count++)
                {
                    switch (dir)
                    {
                        case Direction.Left:
                            currentPosition.x--;
                            break;
                        case Direction.Right:
                            currentPosition.x++;
                            break;
                        case Direction.Up:
                            currentPosition.y--;
                            break;
                        case Direction.Down:
                            currentPosition.y++;
                            break;
                    }
                    if (currentPosition.x == -radius && currentPosition.y == -radius) break;
                    offsetList.Add((currentPosition.x, currentPosition.y));
                }
            }
            offsetMap[radius] = offsetList;
        }
        foreach(var radius in offsetMap.Keys)
        {
            foreach (var offset in offsetMap[radius])
                Debug.LogError($"{radius} {offset}");
        }
    }


    public static (int x, int y) GetRotatedOffset(int angle, Vector2Int original)
    {
        var radius = Math.Max(Math.Abs(original.x), Math.Abs(original.y));
        if (maxRadius < radius)
        {
            maxRadius = radius;
            offsetMap = null;
        }
        if (offsetMap == null) InitializeOffsetMap();

        if (!offsetMap.ContainsKey(radius))
        {
            Debug.LogError($"{radius} is not in offset map");
            return (original.x, original.y);
        }
        var index = offsetMap[radius].IndexOf((original.x, original.y));
        var count = (int)Math.Round(angle / 45f);
        index += count * radius;
        try
        {
            if (index >= offsetMap[radius].Count) index %= offsetMap[radius].Count;
            else if (index < 0) index += offsetMap[radius].Count;
            return offsetMap[radius][index];
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return (original.x, original.y);
        }
    }

    public void Initialize(int maxSize)
    {
        if (maxSize % 2 == 0 || maxSize < 3)
            throw new Exception("Invalid argment exception, size is over 3 odd number");
        MaxSize = maxSize;
        center = Vector2Int.one * maxSize / 2;
        center.x = Mathf.FloorToInt(center.x);
        center.y = Mathf.FloorToInt(center.y);
        data = new AttackData[maxSize, maxSize];
        for (var x = 0; x < maxSize; x++)
            for (var y = 0; y < maxSize; y++)
                data[x, y] = new AttackData() { Offset = new Vector2Int(x, y) - center };
        Strip();
    }

    public void Strip()
    {
        var radius = Mathf.FloorToInt(MaxSize / 2);
        stripedData = new List<AttackData>[radius];
        foreach(var index in Enumerable.Range(0, radius))
            stripedData[index] = new List<AttackData>();

        for (var i = 1; i <= radius; i++)
        {
            var currentPosition = new Vector2Int(-i, -i);// ‰ŠúˆÊ’u‚Í”¼Œa1‚Ì¶ã
            var index = currentPosition + center;
            stripedData[i].Add(data[index.x, index.y]);
            foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                for (var j = 0; j < i + 1; j++)
                {
                    switch (dir)
                    {
                        case Direction.Left:
                            currentPosition.x--;
                            break;
                        case Direction.Right:
                            currentPosition.x++;
                            break;
                        case Direction.Up:
                            currentPosition.y--;
                            break;
                        case Direction.Down:
                            currentPosition.y++;
                            break;
                    }
                    if (currentPosition.x == -i && currentPosition.y == -i) break;
                    index = currentPosition + center;
                    stripedData[i].Add(data[index.x, index.y]);
                }
            }
        }
    }
}
