using System.Collections.Generic;
using UnityEngine;

class Path
{
    public enum Direction
    {
        UP = 0,
        DOWN,
        LEFT,
        RIGHT,
        MAX
    }


    public int ToAreaId { get; set; }
    public Vector2Int To { get; set; }
    public Vector2Int From { get; set; }
    public List<Vector2Int> PathPositonList { get; private set; }
    public Direction Dir { get; set; }

    public int FromAreaId { get; set; }

    public void CreatePositionList(Area fromArea)
    {
        var borderPosition = 0;
        switch (Dir)
        {
            case Direction.UP:
                borderPosition = fromArea.Y;
                break;
            case Direction.DOWN:
                borderPosition = fromArea.Y + fromArea.Height;
                break;
            case Direction.LEFT:
                borderPosition = fromArea.X;
                break;
            case Direction.RIGHT:
                borderPosition = fromArea.X + fromArea.Width;
                break;
        }

        var fromPosition = new Vector2Int();
        var toPosition = new Vector2Int();

        fromPosition.x = Mathf.Min(From.x, To.x);
        fromPosition.y = Mathf.Min(From.y, To.y);

        toPosition.x = Mathf.Max(From.x, To.x);
        toPosition.y = Mathf.Max(From.y, To.y);

        PathPositonList = new List<Vector2Int>();
        if (Dir == Direction.UP || Dir == Direction.DOWN)
        {
            var x = From.x;
            if (From.y < To.y)
            {
                for (int y = From.y; y <= To.y; y++)
                {
                    if (y == borderPosition)
                    {
                        if (From.x < To.x)
                        {
                            for (x = From.x; x < To.x; x++)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                        else
                        {
                            for (x = From.x; x > To.x; x--)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                    }
                    PathPositonList.Add(new Vector2Int(x, y));
                    Debug.Log("(" + x + "," + y + ")");
                }
            }
            else
            {
                for (var y = From.y; y >= To.y; y--)
                {
                    if (y == borderPosition)
                    {
                        if (From.x < To.x)
                        {
                            for (x = From.x; x < To.x; x++)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                        else
                        {
                            for (x = From.x; x > To.x; x--)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                    }
                    PathPositonList.Add(new Vector2Int(x, y));
                    Debug.Log("(" + x + "," + y + ")");
                }
            }
        }
        else
        {
            var y = From.y;
            if (From.x < To.x)
            {
                for (var x = From.x; x <= To.x; x++)
                {
                    if (x == borderPosition)
                    {
                        if (From.y < To.y)
                        {
                            for (y = From.y; y < To.y; y++)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                        else
                        {
                            for (y = From.y; y > To.y; y--)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                    }
                    PathPositonList.Add(new Vector2Int(x, y));
                    Debug.Log("(" + x + "," + y + ")");
                }
            }
            else
            {
                for (var x = From.x; x >= To.x; x--)
                {
                    if (x == borderPosition)
                    {
                        if (From.y < To.y)
                        {
                            for (y = From.y; y < To.y; y++)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                        else
                        {
                            for (y = From.y; y > To.y; y--)
                            {
                                PathPositonList.Add(new Vector2Int(x, y));
                                Debug.Log("(" + x + "," + y + ")");
                            }
                        }
                    }
                    PathPositonList.Add(new Vector2Int(x, y));
                    Debug.Log("(" + x + "," + y + ")");
                }
            }
        }
    }

}
