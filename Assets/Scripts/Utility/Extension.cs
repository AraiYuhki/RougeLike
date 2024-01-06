using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System.Drawing;


public static class Extension
{
    private static Random random = new Random();
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

    public static T[,] To2DArray<T>(this T[] self, Vector2Int size)
        => self.To2DArray(size.x, size.y);
    

    public static T[,] To2DArray<T>(this T[] self, int width, int height)
    {
        var newData = new T[width, height];
        var index = 0;
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (index >= self.Length)
                {
                    newData[x, y] = default;
                    continue;
                }
                newData[x, y] = self[index];
                index++;
            }
        }
        return newData;
    }

    public static T Random<T>(this IEnumerable<T> self)
    {
        var value = random.Next(self.Count());
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

    public static IEnumerable<T> Randmize<T>(this IList<T> values) => values.OrderBy(_ => Guid.NewGuid());
    public static IEnumerable<T> Randmize<T>(this IEnumerable<T> values) => values.OrderBy(_ => Guid.NewGuid());

    public static void Shuffle<T>(this IList<T> values)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var nextIndex = random.Next(index + 1);
            var value = values[nextIndex];
            values[nextIndex] = values[index];
            values[index] = value;
        }
    }

    public static int IndexOf<T>(this IEnumerable<T> values, Func<T, bool> action)
    {
        foreach ((var value, var index) in values.Select((value, index) => (value, index)))
        {
            if (action(value))
                return index;
        }
        return -1;
    }

    public static Vector2 ToVector2(this Vector2Int self) => new Vector2(self.x, self.y);
    public static Vector3 ToVector3(this Vector2 self, float z = 0f) => new Vector3(self.x, self.y, z);
    public static Vector3 ToVector3(this Vector2Int self, float z = 0f) => new Vector3(self.x, self.y, z);
}
