using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class IndexedItem2<T>
{
    public T Value { get; }
    public int x { get; }
    public int y { get; }
    public Vector2Int position => new Vector2Int(x, y);
    public IndexedItem2(T value, int x, int y)
    {
        Value = value;
        this.x = x;
        this.y = y;
    }
}

public static class Extension
{ 
    public static List<T> ToList<T>(this T[,] self)
    {
        var xLength = self.GetLength(0);
        var yLength = self.GetLength(1);
        var result = new List<T>();
        for (var y = 0; y < yLength; y++)
        {
            for (var x = 0; x < xLength; x++)
            {
                result.Add(self[x, y]);
            }
        }
        return result;
    }
    public static T[] ToArray<T>(this T[,] self)
    {
        return self.ToList().ToArray();
    }

    public static IEnumerable<IndexedItem2<T>> WithIndex<T>(this T[,] self)
    {
        if (self == null)
            throw new ArgumentNullException(nameof(self));
        for (var x = 0; x < self.GetLength(0); x++)
            for (var y = 0; y < self.GetLength(1); y++)
                yield return new IndexedItem2<T>(self[x, y], x, y);
    }

    public static T Random<T>(this IEnumerable<T> self)
    {
        var value = UnityEngine.Random.Range(0, self.Count());
        try
        {
            return self.ElementAt(value);
        }
        catch (Exception e)
        {
            Debug.LogError($"{value} : {self.Count()} : {e.Message}");
            throw e;
        }
    }

    public static Vector2 ToVector2(this Vector2Int self) => new Vector2(self.x, self.y);
    public static Vector3 ToVector3(this Vector2 self, float z = 0f) => new Vector3(self.x, self.y, z);
    public static Vector3 ToVector3(this Vector2Int self, float z = 0f) => new Vector3(self.x, self.y, z);
}
