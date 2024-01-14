using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 抽選システム
/// </summary>
public static class Lottery
{
    public static T Get<T>(IEnumerable<T> array) where T : class, ILotterable
    {
        var total = array.Sum(value => value.Probability);
        var target = Random.Range(0, total);
        return Get(array, target);
    }

    public static T Get<T>(IEnumerable<T> array, int target) where T : class, ILotterable
    {
        if (target < 0) return null;
        var value = 0;
        foreach (var row in array)
        {
            value += row.Probability;
            if (value >= target) return row;
        }
        return null;
    }
}
