using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    [SerializeField]
    private int dungeonId;
    [SerializeField]
    private int currentFloor;
    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private List<ItemData> items;
    [SerializeField]
    private List<TrapData> traps;
    [SerializeField]
    private List<EnemyData> enemies;
    [SerializeField]
    private FloorData floorData;
    [SerializeField]
    private List<int> deck;
    [SerializeField]
    private List<int> hands;
    [SerializeField]
    private List<int> cemetary;
    [SerializeField]
    private bool[] visibleTiles;

    public int DungeonId => dungeonId;
    public int CurrentFloor => currentFloor;
    public PlayerData PlayerData => playerData;
    public List<ItemData> Items => items;
    public List<TrapData> Traps => traps;
    public List<EnemyData> Enemies => enemies;
    public FloorData FloorData => floorData;
    public List<int> Deck => deck;
    public List<int> Hands => hands;
    public List<int> Cemetary => cemetary;
    public bool[] VisibleTiles => visibleTiles;

    public SaveData(
        int dungeonId,
        int currentFloor,
        PlayerData playerData,
        List<ItemData> items,
        List<TrapData> traps,
        List<EnemyData> enemies,
        FloorData floorData,
        CardController cardController,
        bool[] visibleTiles
        )
    {
        this.dungeonId = dungeonId;
        this.currentFloor = currentFloor;
        this.playerData = playerData;
        this.items = items;
        this.traps = traps;
        this.enemies = enemies;
        this.floorData = floorData;
        (deck, hands, cemetary) = cardController.GetSerializableData();
        this.visibleTiles = visibleTiles;
    }
}
