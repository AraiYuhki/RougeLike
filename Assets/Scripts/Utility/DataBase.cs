using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    private const string PrefabPath = "Prefabs/DB";
    private static DataBase instance;
    public static DataBase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load<DataBase>(PrefabPath));
                instance.name = "DB";
            }
            return instance;
        }
    }
    [SerializeField]
    private List<ScriptableObject> tables = new List<ScriptableObject>();
    private Dictionary<Type, ScriptableObject> tableDictionary = new Dictionary<Type, ScriptableObject>();
    private void Awake()
    {
        foreach(var table in tables)
            tableDictionary.Add(table.GetType(), table);
    }

    public void Initialize()
    {
        if (tableDictionary != null && tableDictionary.Count > 0) return;
        tableDictionary = tables.ToDictionary(table => table.GetType(), table => table);
    }

    public T GetTable<T>() where T : ScriptableObject
    {
        if (tableDictionary.ContainsKey(typeof(T)))
            return (T)tableDictionary[typeof(T)];
        return null;
    }
}
