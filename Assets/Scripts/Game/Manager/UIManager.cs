using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private DungeonUI dungeonUI;
    [SerializeField]
    private MenuUI menuUI;

    public Action OnCloseMenu 
    {
        get => menuUI.OnClose;
        set => menuUI.OnClose = value;
    }

    public bool IsMenuOpened => menuUI.IsOpened;

    private Minimap minimap => dungeonUI.Minimap;
    private InventoryUI inventoryUI => menuUI.InventoryUI;

    public void Initialize(FloorManager floorManager, Player player, Action onUsed, Action onTakeItem, Action<ItemBase> onThrowItem, Action<ItemBase> onDropItem)
    {
        floorManager.SetMinimap(dungeonUI.Minimap);
        menuUI.Initialize(player, onUsed, onTakeItem, onThrowItem, onDropItem);
    }

    public void OpenMenu(TweenCallback onComplete)
    {
        dungeonUI.Minimap.SetMode(MinimapMode.Menu);
        menuUI.Open(onComplete);
    }
    public void CloseMenu(TweenCallback onComplete = null)
    {
        dungeonUI.Minimap.SetMode(MinimapMode.Normal);
        menuUI.Close(onComplete);
    }
    public void UpdateUI() => menuUI.Controll();
}
