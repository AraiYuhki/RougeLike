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
    private MainMenuUI mainMenu;
    [SerializeField]
    private InventoryUI inventoryUI;
    [SerializeField]
    private StatusUI statusUI;
    [SerializeField]
    private UseMenuUI useMenuUI;

    public InventoryUI InventoryUI => inventoryUI;
    public StatusUI StatusUI => statusUI;
    public Minimap minimap => ServiceLocator.Instance.DungeonUI.Minimap;

    public Action OnClose { get; set; }

    private Player Player => ServiceLocator.Instance.GameController.Player;
    private PlayerData Data => Player.Data;
    private bool isOpened = false;
    private ControllType type = ControllType.Inventory;
    private Action onUsed;
    private Action<ItemBase> onDropItem;
    private Action<ItemBase> onThrowItem;
    private Action onTakeItem;
    private Action onEquiped;

    public void Initialize(Player player, Action onUsed, Action onTakeItem, Action<ItemBase> onThrowItem, Action<ItemBase> onDropItem)
    {
        mainMenu.Initialize(
            OpenInventory, 
            CheckStep,
            Suspend,
            Retire
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
            isOpened = true;
        });
        minimap.SetMode(MinimapMode.Menu);
    }

    public void Close(TweenCallback onComplete = null)
    {
        mainMenu.Close();
        inventoryUI.Close();
        useMenuUI.Close();
        statusUI.Close(() => {
            isOpened = false;
            onComplete?.Invoke();
            OnClose?.Invoke();
        });
        minimap.SetMode(MinimapMode.Normal);
    }

    public void SwitchOpenMenu(TweenCallback onComplete)
    {
        if (!isOpened)
            Open(onComplete);
        else
            Close(onComplete);
    }

    public IEnumerator Controll()
    {
        switch(type)
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
        yield return null;
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
            if (ServiceLocator.Instance.FloorManager.GetItem(Player.Position) == null)
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
            ServiceLocator.Instance.DungeonUI.Minimap.SetMode(MinimapMode.Menu);
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
                Player.Heal(data.Parameter);
                break;
            case ItemType.PowerUp:
                Player.PowerUp((int)data.Parameter);
                break;
            case ItemType.StaminaHeal:
                Player.RecoveryStamina(data.Parameter);
                break;
            default:
                throw new NotImplementedException();
        }
        Debug.LogError($"{data.Name}:を使用した");
        if (!data.IsStackable)
        {
            Data.Inventory.Remove(data);
            Close(() => onUsed?.Invoke());
            return;
        }

        Data.Inventory[data]--;
        if (Data.Inventory[data] <= 0)
            Player.Data.Inventory.Remove(data);
        Close(() => onUsed?.Invoke());
    }

    private void EquipWeapon(WeaponData weapon)
    {
        Player.Data.EquipmentWeapon = weapon;
        Debug.LogError($"{weapon.Name}を装備した");
        inventoryUI.UpdateStatus();
        CloseUseMenu();
    }

    private void EquipShield(ShieldData shield)
    {
        Player.Data.EquipmentShield = shield;
        Debug.LogError($"{shield.Name}を装備した");
        inventoryUI.UpdateStatus();
        CloseUseMenu();
    }

    private void ThrowItem()
    {
        var target = inventoryUI.SelectedItem;
        Player.Data.Inventory.Remove(target);
        Debug.LogError($"{target.Name}を投げた");
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
        Player.Data.Inventory.Remove(target);
        Debug.LogError($"{target.Name}を地面に置いた");
        Close(() => onDropItem?.Invoke(target));
    }
}
