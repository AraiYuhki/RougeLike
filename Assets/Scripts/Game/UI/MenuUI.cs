using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuUI : MonoBehaviour
{
    enum ControllType
    {
        Inventory,
        UseMenu,
    }

    [SerializeField]
    private InventoryUI inventoryUI;
    [SerializeField]
    private StatusUI statusUI;
    [SerializeField]
    private UseMenuUI useMenuUI;

    public InventoryUI InventoryUI => inventoryUI;
    public StatusUI StatusUI => statusUI;
    private Player Player => ServiceLocator.Instance.GameController.Player;
    private PlayerData Data => Player.Data;
    private bool isOpened = false;
    private ControllType type = ControllType.Inventory;
    private InGameInput gameInput;
    private Action usedCallback;

    public void Initialize(Player player, InGameInput gameInput, Action usedCallback)
    {
        inventoryUI.Initialize(player);
        statusUI.Initialize(player);
        this.gameInput = gameInput;
        this.usedCallback = usedCallback;
    }

    public void Open(TweenCallback onComplete = null)
    {
        inventoryUI.Open();
        statusUI.Open(() =>
        {
            onComplete?.Invoke();
            isOpened = true;
        });
    }

    public void Close(TweenCallback onComplete = null)
    {
        inventoryUI.Close();
        useMenuUI.Close();
        statusUI.Close(() => {
            isOpened = false;
            onComplete?.Invoke();
        });
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
            case ControllType.Inventory:
                InventoryControll();
                break;
            case ControllType.UseMenu:
                UseMenuControll();
                break;
        }
        yield return null;
    }

    private void InventoryControll()
    {
        if (gameInput.Player.Up.WasPressedThisFrame())
            inventoryUI.Up();
        else if (gameInput.Player.Down.WasPressedThisFrame())
            inventoryUI.Down();

        if (gameInput.Player.Submit.WasPressedThisFrame())
        {
            var menu = new List<(string, UnityAction)>();
            if (inventoryUI.SelectedItem is WeaponData weapon)
            {
                menu.Add(("‘•”õ", () => Player.Data.EquipmentWeapon = weapon));
            }
            else if (inventoryUI.SelectedItem is ShieldData shield)
            {
                menu.Add(("‘•”õ", () => Player.Data.EquipmentShield = shield));
            }
            else if (inventoryUI.SelectedItem is UsableItemData data)
            {
                menu.Add(("Žg‚¤", () => { UseItem(data); }));
            }
            menu.Add(("“Š‚°‚é", () => { }));
            menu.Add(("’u‚­", () => { }));
            menu.Add(("–ß‚é", () =>
            {
                useMenuUI.Close();
                type = ControllType.Inventory;
            }
            ));
            useMenuUI.Initialize(menu);
            useMenuUI.Open();
            type = ControllType.UseMenu;
        }
    }

    private void UseMenuControll()
    {
        if (gameInput.Player.Up.WasPressedThisFrame())
            useMenuUI.Up();
        else if (gameInput.Player.Down.WasPressedThisFrame())
            useMenuUI.Down();
        if (gameInput.Player.Submit.WasPressedThisFrame())
            useMenuUI.Submit();
        else if (gameInput.Player.Cancel.WasPressedThisFrame())
        {
            useMenuUI.Close();
            type = ControllType.Inventory;
        }
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
        if (!data.IsStackable)
        {
            Data.Inventory.Remove(data);
            Close(() => usedCallback?.Invoke());
            return;
        }

        Data.Inventory[data]--;
        if (Data.Inventory[data] <= 0)
            Player.Data.Inventory.Remove(data);
        Close(() => usedCallback?.Invoke());
    }
}
