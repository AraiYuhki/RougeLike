using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class InventoryUI : ScrollMenu
{
    [SerializeField]
    private Canvas canvas;

    private Player player;
    private PlayerData data => player.Data;
    public CanvasGroup Group => group;

    public ItemBase SelectedItem => (items[selectedIndex] as ItemRowController).ItemData;
    public ItemRowController SelectedRow => items[selectedIndex] as ItemRowController;

    public override void Open(UnityAction onComplete = null)
    {
        canvas.enabled = true;
        base.Open(onComplete);
    }

    public override void Close(UnityAction onComplete = null)
    {
        base.Close(() =>
        {
            onComplete?.Invoke();
            canvas.enabled = false;
        });
    }

    public void Initialize(Player player)
    {
        this.player = player;
        Initialize();
    }

    public void Initialize()
    {
        Clean();
        foreach ((var pair, var index) in data.Inventory.Select((v, index) => (v, index)))
        {
            var item = Instantiate(template as ItemRowController, baseObject);
            item.gameObject.SetActive(true);
            var isEquip = data.EquipmentWeapon == pair.Key || data.EquipmentShield == pair.Key;
            item.Initialize(pair.Key, pair.Value, isEquip, () => SetSelectIndex(index));
            items.Add(item);
        }
    }

    public void UpdateStatus()
    {
        foreach (ItemRowController item in items)
        {
            var count = player.Data.Inventory[item.ItemData];
            if (player.Data.EquipmentWeapon == item.ItemData || player.Data.EquipmentShield == item.ItemData)
                item.UpdateStatus(true, count);
            else
                item.UpdateStatus(false, count);
        }
    }
}
