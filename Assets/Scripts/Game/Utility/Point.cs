using System;
using UnityEngine;

[Serializable]
public class Point
{
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public Point()
    {
        x = 0;
        y = 0;
    }
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Point(float x, float y)
    {
        this.x = (int)x;
        this.y = (int)y;
    }

    public static Point operator +(Point a, Point b) => new Point(a.x + b.x, a.y + b.y);
    public static Point operator +(Point a, Vector2Int b) => new Point(a.x + b.x, a.y + b.y);
    public static Point operator +(Vector2Int a, Point b) => new Point(a.x + b.x, a.y + b.y);
    public static Vector2 operator +(Point a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
    public static Point operator -(Point a, Point b) => new Point(a.x - b.x, a.y - b.y);
    public static Point operator -(Point a, Vector2Int b) => new Point(a.x - b.x, a.y - b.y);
    public static Point operator -(Vector2Int a, Point b) => new Point(a.x - b.x, a.y - b.y);
    public static Point operator *(Point a, float b) => new Point(a.x * b, a.y * b);
    public static Point operator *(float a, Point b) => new Point(a * b.x, a * b.y);
    public static Point operator /(Point a, float b) => new Point(a.x / b, a.y / b);
    public static bool operator ==(Point a, Point b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Point a, Point b) => !(a == b);
    public static implicit operator Vector2(Point self) => new Vector2(self.x, self.y);
    public static implicit operator Vector3(Point self) => new Vector3(self.x, self.y);
    public static implicit operator Vector2Int(Point self) => new Vector2Int(self.x, self.y);
    public static implicit operator Point(Vector2Int self) => new Point(self.x, self.y);

    public override bool Equals(object obj)
    {
        return obj is Point point &&
               x == point.x &&
               y == point.y &&
               X == point.X &&
               Y == point.Y;
    }

    public override int GetHashCode() => HashCode.Combine(x, y, X, Y);
    public override string ToString() => $"({x}, {y})";
}
