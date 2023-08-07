using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


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

    public static Vector2 ToVector2(this Vector2Int self) => new Vector2(self.x, self.y);
    public static Vector3 ToVector3(this Vector2 self, float z = 0f) => new Vector3(self.x, self.y, z);
    public static Vector3 ToVector3(this Vector2Int self, float z = 0f) => new Vector3(self.x, self.y, z);
}
