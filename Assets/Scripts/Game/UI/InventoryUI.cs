using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryUI : ScrollMenu
{
    private Player player;
    private PlayerData data => player.Data;
    public CanvasGroup Group => group;

    public ItemBase SelectedItem => (items[selectedIndex] as ItemRowController).ItemData;

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
            item.Initialize(pair.Key, pair.Value, () => SetSelectIndex(index));
            items.Add(item);
        }
    }
}
