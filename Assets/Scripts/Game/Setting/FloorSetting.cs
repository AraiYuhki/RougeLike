using System;
using UnityEngine;

[Serializable]
public class FloorSetting
{
    [SerializeField]
    private int sameSettingCount = 0;
    [SerializeField]
    private Vector2Int size = new Vector2Int(20, 20);
    [SerializeField]
    private int maxRoomCount = 3;
    [SerializeField, Range(0f, 1f)]
    private float deletePathProbability;
    [SerializeField]
    private Material floorMaterial;
    [SerializeField]
    private Material wallMaterial;
    public int SameSettingCount => sameSettingCount;
    public Vector2Int Size => size;
    public int MaxRoomCount => maxRoomCount;
    public float DeletePathProbability => deletePathProbability;
    public Material FloorMaterial => floorMaterial;
    public Material WallMaterial => wallMaterial;
}
