using System.Collections.Generic;
using UnityEngine;

public class DungeonData
{
    [SerializeField]
    private List<FloorData> floorList = new List<FloorData>();

    public List<FloorData> FloorList { get => floorList; set => floorList = value; }
}
