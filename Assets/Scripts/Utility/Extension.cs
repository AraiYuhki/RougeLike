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
        return self.ElementAt(value);
    }
}
