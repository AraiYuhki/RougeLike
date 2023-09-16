using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class AttackAreaInfo
{
    private enum Direction
    {
        Right,
        Down,
        Left,
        Up,
    }

    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("memo")]
    private string memo;
    [SerializeField, CsvColumn("maxSize")]
    private int maxSize;
    [SerializeField, CsvColumn("center")]
    private Vector2Int center;
    [SerializeField, CsvColumn("attackGroupId")]
    private int attackGroupId;

    private List<AttackInfo> data;

    private static int maxRadius = 10;
    private static Dictionary<int, List<(int x, int y)>> offsetMap;

    public int Id => id;
    public int MaxSize => MaxSize;
    public Vector2Int Center => center;
    public int AttackGroupId => attackGroupId;

    public List<AttackInfo> Data
    {
        get
        {
            if (data == null)
                data = DB.Instance.MAttack.GetByGroupId(AttackGroupId);
            return data;
        }
    }

    public AttackAreaInfo() { }

    public static void InitializeOffsetMap()
    {
        var halfRadius = Mathf.FloorToInt(maxRadius / 2);
        offsetMap = new Dictionary<int, List<(int x, int y)>>();
        foreach (var index in Enumerable.Range(1, halfRadius + 1))
            offsetMap[index] = new List<(int x, int y)>();
        for (var radius = 1; radius <= maxRadius; radius++)
        {
            var currentPosition = new Vector2Int(-radius, -radius);
            var offsetList = new List<(int x, int y)>() { (currentPosition.x, currentPosition.y) };
            foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                for (var count = 0; count < radius * 2; count++)
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
    }

    public static (int x, int y) GetRotatedOffset(float angle, Vector2Int original)
    {
        var radius = Math.Max(Math.Abs(original.x), Math.Abs(original.y));
        if (radius == 0) return (original.x, original.y);
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
#if DEBUG
    public string Memo
    {
        get => memo;
        set => memo = value;
    }

    public AttackAreaInfo(int id, int groupId)
    {
        this.id = id;
        this.attackGroupId = groupId;
    }
    public void SetData(List<AttackInfo> data)
    {
        this.data = data;
    }

    public AttackAreaInfo Clone()
    {
        return new AttackAreaInfo
        {
            id = this.id,
            attackGroupId = this.attackGroupId,
            memo = this.memo,
            data = new List<AttackInfo>(),
        };
    }
#endif
}
