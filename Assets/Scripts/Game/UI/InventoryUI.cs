using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System;

public class InventoryUI : ScrollMenu
{
    [SerializeField]
    private UnityEvent onUseItem;
    [SerializeField]
    private UnityEvent onTakeItem;
    [SerializeField]
    private UnityEvent<ItemBase> onThrowItem;
    [SerializeField]
    private UnityEvent<ItemBase> onDropItem;

    private FloorManager floorManager;
    private UIManager uiManager;
    private NoticeGroup notice;
    private Player player;
    private PlayerData data => player.Data;
    public CanvasGroup Group => group;

    public ItemBase SelectedItem => (items[selectedIndex] as ItemRowController).ItemData;
    public ItemRowController SelectedRow => items[selectedIndex] as ItemRowController;

    public void Initialize(FloorManager floorManager, UIManager uiManager, NoticeGroup notice, Player player)
    {
        this.floorManager = floorManager;
        this.uiManager = uiManager;
        this.notice = notice;
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

    public override void Submit()
    {
        var menu = new List<(string, Action)>();
        if (SelectedItem is WeaponData || SelectedItem is ShieldData)
            menu.Add(("装備", Equip));
        else if (SelectedItem is UsableItemData item)
            menu.Add(("使う", UseItem));
        menu.Add(("投げる", ThrowItem));
        if (floorManager.GetItem(player.Position) == null)
            menu.Add(("置く", DropItem));
        else
            menu.Add(("拾う", TakeItem));
        menu.Add(("戻る", () =>
        {
            uiManager.CloseCurrent();
        }));
        uiManager.OpenUseMenu(menu);
    }

    private void Equip()
    {
        var item = SelectedItem;
        if (item is WeaponData weapon)
            player.Data.EquipmentWeapon = weapon;
        else if (item is ShieldData shield)
            player.Data.EquipmentShield = shield;
        else
            throw new Exception("装備できないアイテムが引数に指定されました");
        notice.Add($"{item.Name}を装備した", Color.green);
        UpdateStatus();
        uiManager.CloseCurrent();
    }

    private void UseItem()
    {
        var item = SelectedItem as UsableItemData;
        switch(item.Type)
        {
            case ItemType.HPHeal:
                player.Heal(item.Parameter);
                break;
            case ItemType.PowerUp:
                player.PowerUp((int)item.Parameter);
                break;
            case ItemType.StaminaHeal:
                player.RecoveryStamina(item.Parameter);
                break;
            default:
                throw new NotImplementedException();
        }
        notice.Add($"{item.Name}を使用した", Color.green);
        if (!item.IsStackable)
            data.Inventory.Remove(item);
        else
        {
            data.Inventory[item]--;
            if (data.Inventory[item] <= 0)
                data.Inventory.Remove(item);
        }
        uiManager.CloseAll(() => onUseItem?.Invoke());
    }

    private void ThrowItem()
    {
        var item = SelectedItem;
        player.Data.Inventory.Remove(item);
        notice.Add($"{item.Name}を投げた", Color.cyan);
        uiManager.CloseAll(() => onThrowItem?.Invoke(item));
    }

    private void TakeItem()
    {
        onTakeItem?.Invoke();
    }

    private void DropItem()
    {
        var item = SelectedItem;
        player.Data.Inventory.Remove(item);
        notice.Add($"{item.Name}を地面においた", Color.green);
        onDropItem?.Invoke(item);
    }
}
