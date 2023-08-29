using System;
using UnityEngine;

[Serializable]
public class DungeonData : ICloneable
{
    [SerializeField, CsvColumn("ID")]
    private int id;
    [SerializeField, CsvColumn("Name")]
    private string name;
    [SerializeField, CsvColumn("IsTower")]
    private bool isTower;

    public int Id => id;
    public string Name => name;
    public bool IsTower => isTower;

    public object Clone()
    {
        return new DungeonData()
        {
            id = id,
            name = name,
            isTower = isTower
        };
    }
}
