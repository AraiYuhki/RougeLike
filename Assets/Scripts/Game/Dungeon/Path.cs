using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Path
{
    public enum Direction
    {
        Up = 0,
        Down,
        Left,
        Right,
        MAX
    }

    [SerializeField]
    private int toAreaId;
    [SerializeField]
    private int fromAreaId;
    [SerializeField]
    private Point to;
    [SerializeField]
    private Point from;
    [SerializeField]
    private List<Point> pathPositionList = new List<Point>();
    [SerializeField]
    private Direction direction;

    public int ToAreaId
    {
        get => toAreaId;
        set => toAreaId = value;
    }

    public int FromAreaId
    {
        get => fromAreaId;
        set => fromAreaId = value;
    }

    public Point To
    {
        get => to;
        set => to = value;
    }
    public Point From
    {
        get => from;
        set => from = value;
    }

    public List<Point> PathPositionList => pathPositionList;
    public Direction Dir
    {
        get => direction;
        set => direction = value;
    }

    public Path() { }

    public void CreatePositionList(Area fromArea)
    {
        var borderPosition = 0;
        switch (Dir)
        {
            case Direction.Up:
                borderPosition = fromArea.Y;
                break;
            case Direction.Down:
                borderPosition = fromArea.Y + fromArea.Height;
                break;
            case Direction.Left:
                borderPosition = fromArea.X;
                break;
            case Direction.Right:
                borderPosition = fromArea.X + fromArea.Width;
                break;
        }

        var fromPosition = new Point();
        var toPosition = new Point();

        fromPosition.X = Mathf.Min(From.X, To.X);
        fromPosition.Y = Mathf.Min(From.Y, To.Y);

        toPosition.X = Mathf.Max(From.X, To.X);
        toPosition.Y = Mathf.Max(From.Y, To.Y);

        pathPositionList = new List<Point>();
        if (Dir == Direction.Up || Dir == Direction.Down)
            CreatePositionListVertical(borderPosition);
        else
            CreatePositionListHorizontal(borderPosition);
    }

    private void CreatePositionListVertical(int borderPosition)
    {
        var x = From.X;
        if (From.Y < To.Y)
        {
            for (int y = From.Y; y <= To.Y; y++)
            {
                if (y == borderPosition) CreatePositionListHorizontal(ref x, y);
                PathPositionList.Add(new Point(x, y));
            }
            return;
        }

        for (var y = From.Y; y >= To.Y; y--)
        {
            if (y == borderPosition) CreatePositionListHorizontal(ref x, y);
            PathPositionList.Add(new Point(x, y));
        }
    }

    private void CreatePositionListVertical(int x, ref int y)
    {
        if (From.Y < To.Y)
        {
            for (y = From.Y; y < To.Y; y++) PathPositionList.Add(new Point(x, y));
            return;
        }
        for (y = From.Y; y > To.Y; y--) PathPositionList.Add(new Point(x, y));
    }

    private void CreatePositionListHorizontal(int borderPosition)
    {
        var y = From.Y;
        if (From.X < To.X)
        {
            for (var x = From.X; x <= To.X; x++)
            {
                if (x == borderPosition) CreatePositionListVertical(x, ref y);
                PathPositionList.Add(new Point(x, y));
            }
            return;
        }

        for (var x = From.X; x >= To.X; x--)
        {
            if (x == borderPosition) CreatePositionListVertical(x, ref y);
            PathPositionList.Add(new Point(x, y));
        }
    }

    private void CreatePositionListHorizontal(ref int x, int y)
    {
        if (From.X < To.X)
        {
            for (x = From.X; x < To.X; x++) PathPositionList.Add(new Point(x, y));
            return;
        }
        for (x = From.X; x > To.X; x--) PathPositionList.Add(new Point(x, y));
    }

}
