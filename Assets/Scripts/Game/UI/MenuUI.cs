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
    
    public void Initialize(Player player, InGameInput gameInput)
    {
        inventoryUI.Initialize(player);
        statusUI.Initialize(player);
        this.gameInput = gameInput;
    }

    public void SwitchOpenMenu(Action onComplete)
    {
        TweenCallback callback = () =>
        {
            isOpened = !isOpened;
            onComplete?.Invoke();
        };
        if (!isOpened)
        {
            inventoryUI.Open();
            statusUI.Open(callback);
        }
        else
        {
            inventoryUI.Close();
            statusUI.Close(callback);
        }
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
            else if (inventoryUI.SelectedItem is UsableItemData)
            {
                menu.Add(("Žg‚¤", () => { }));
            }
            menu.Add(("“Š‚°‚é", () => { }));
            menu.Add(("’u‚­", () => { }));
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
}
