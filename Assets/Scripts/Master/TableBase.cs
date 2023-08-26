using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class TableBase<T> : ScriptableObject where T : class, ICloneable
{
    protected static string MasterDirectory => System.IO.Path.Combine(Application.dataPath, "Master", TableName + ".json");
    public static string TableName { get; }
    [SerializeField]
    private List<T> data = new List<T>();

    public List<T> Data => data;
    public T GetData(int id) => data[id];

    public T Get(int id) => id >= 0 && id < data.Count ?  data[id].Clone() as T : null;
}
