using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuUI : MonoBehaviour
{
    enum ControllType
    {
        Main,
        Inventory,
        UseMenu,
    }
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private Player player;
    [SerializeField]
    private NoticeGroup notice;
    [SerializeField]
    private Minimap minimap;
    [SerializeField]
    private MainMenuUI mainMenu;
    [SerializeField]
    private InventoryUI inventoryUI;
    [SerializeField]
    private StatusUI statusUI;
    [SerializeField]
    private UseMenuUI useMenuUI;
    [SerializeField]
    private SelectableItem closeButton;

    public InventoryUI InventoryUI => inventoryUI;
    public StatusUI StatusUI => statusUI;

    public Action OnClose { get; set; }

    private PlayerData Data => player.Data;
    private ControllType type = ControllType.Inventory;
    private Action onUsed;
    private Action<ItemBase> onDropItem;
    private Action<ItemBase> onThrowItem;
    private Action onTakeItem;
    private Action onEquiped;

    public bool IsOpened { get; set; } = false;

    public void Initialize(Player player, Action onUsed, Action onTakeItem, Action<ItemBase> onThrowItem, Action<ItemBase> onDropItem)
    {
        mainMenu.Initialize(
            OpenInventory, 
            CheckStep,
            Suspend,
            Retire,
            () => Close()
            );
        inventoryUI.Initialize(player);
        statusUI.Initialize(player);
        this.onUsed = onUsed;
        this.onTakeItem = onTakeItem;
        this.onThrowItem = onThrowItem;
        this.onDropItem = onDropItem;
    }

    public void Open(TweenCallback onComplete = null)
    {
        statusUI.Open();
        mainMenu.Open(() =>
        {
            type = ControllType.Main;
            onComplete?.Invoke();
            IsOpened = true;
        });
        minimap.SetMode(MinimapMode.Menu);
    }

    public void Close(TweenCallback onComplete = null)
    {
        mainMenu.Close();
        inventoryUI.Close();
        useMenuUI.Close();
        statusUI.Close(() => {
            IsOpened = false;
            onComplete?.Invoke();
            OnClose?.Invoke();
        });
        minimap.SetMode(MinimapMode.Normal);
    }

    public void SwitchOpenMenu(TweenCallback onComplete)
    {
        if (!IsOpened)
            Open(onComplete);
        else
            Close(onComplete);
    }

    public void Controll()
    {
        switch (type)
        {
            case ControllType.Main:
                MainMenuControll();
                break;
            case ControllType.Inventory:
                InventoryControll();
                break;
            case ControllType.UseMenu:
                UseMenuControll();
                break;
        }
    }

    private void OpenInventory()
    {
        inventoryUI.Initialize();
        inventoryUI.Open(() => type = ControllType.Inventory);
        minimap.SetMode(MinimapMode.Normal);
    }

    private void CheckStep()
    {

    }

    private void Suspend()
    {

    }

    private void Retire()
    {

    }
    private void MainMenuControll()
    {
        if (InputUtility.Up.IsTriggerd())
            mainMenu.Up();
        else if (InputUtility.Down.IsTriggerd())
            mainMenu.Down();
        if (InputUtility.Submit.IsTriggerd())
            mainMenu.Submit();
        else if (InputUtility.Cancel.IsTriggerd())
            Close();
    }

    private void InventoryControll()
    {
        if (InputUtility.Up.IsTriggerd())
            inventoryUI.Up();
        else if (InputUtility.Down.IsTriggerd())
            inventoryUI.Down();

        if (InputUtility.Submit.IsTriggerd())
        {
            var menu = new List<(string, UnityAction)>();
            if (inventoryUI.SelectedItem is WeaponData weapon)
            {
                menu.Add(("装備", () => EquipWeapon(weapon)));
            }
            else if (inventoryUI.SelectedItem is ShieldData shield)
            {
                menu.Add(("装備", () => EquipShield(shield)));
            }
            else if (inventoryUI.SelectedItem is UsableItemData data)
            {
                menu.Add(("使う", () => UseItem(data)));
            }
            menu.Add(("投げる", () => { ThrowItem(); }));
            if (floorManager.GetItem(player.Position) == null)
                menu.Add(("置く", DropItem));
            menu.Add(("戻る", () =>
            {
                useMenuUI.Close();
                type = ControllType.Inventory;
            }
            ));
            useMenuUI.Initialize(menu);
            useMenuUI.Open();
            type = ControllType.UseMenu;
        }
        else if (InputUtility.Cancel.IsTriggerd())
        {
            minimap.SetMode(MinimapMode.Menu);
            inventoryUI.Close(() => type = ControllType.Main);
        }
    }

    private void UseMenuControll()
    {
        if (InputUtility.Up.IsTriggerd())
            useMenuUI.Up();
        else if (InputUtility.Down.IsTriggerd())
            useMenuUI.Down();
        if (InputUtility.Submit.IsTriggerd())
            useMenuUI.Submit();
        else if (InputUtility.Cancel.IsTriggerd())
            CloseUseMenu();
    }

    private void CloseUseMenu()
    {
        useMenuUI.Close();
        type = ControllType.Inventory;
    }

    private void UseItem(UsableItemData data)
    {
        switch(data.Type)
        {
            case ItemType.HPHeal:
                player.Heal(data.Parameter);
                break;
            case ItemType.PowerUp:
                player.PowerUp((int)data.Parameter);
                break;
            case ItemType.StaminaHeal:
                player.RecoveryStamina(data.Parameter);
                break;
            default:
                throw new NotImplementedException();
        }
        notice.Add($"{data.Name}:を使用した", Color.green);
        if (!data.IsStackable)
        {
            Data.Inventory.Remove(data);
            Close(() => onUsed?.Invoke());
            return;
        }

        Data.Inventory[data]--;
        if (Data.Inventory[data] <= 0)
            player.Data.Inventory.Remove(data);
        Close(() => onUsed?.Invoke());
    }

    private void EquipWeapon(WeaponData weapon)
    {
        player.Data.EquipmentWeapon = weapon;
        notice.Add($"{weapon.Name}を装備した", Color.green);
        inventoryUI.UpdateStatus();
        CloseUseMenu();
    }

    private void EquipShield(ShieldData shield)
    {
        player.Data.EquipmentShield = shield;
        notice.Add($"{shield.Name}を装備した", Color.green);
        inventoryUI.UpdateStatus();
        CloseUseMenu();
    }

    private void ThrowItem()
    {
        var target = inventoryUI.SelectedItem;
        player.Data.Inventory.Remove(target);
        notice.Add($"{target.Name}を投げた", Color.cyan);
        Close(() => onThrowItem?.Invoke(target));
    }

    private void TakeItem()
    {
        onTakeItem?.Invoke();
        Close();
    }

    private void DropItem()
    {
        var target = inventoryUI.SelectedItem;
        player.Data.Inventory.Remove(target);
        notice.Add($"{target.Name}を地面に置いた", Color.green);
        Close(() => onDropItem?.Invoke(target));
    }
}
