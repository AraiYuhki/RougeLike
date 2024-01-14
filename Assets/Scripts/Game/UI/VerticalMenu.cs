using System;
using System.Collections.Generic;
using UnityEngine;

public class VerticalMenu : MonoBehaviour
{
    [SerializeField]
    private Transform container;

    private int selectedIndex = 0;
    public bool Enable { get; set; }
    public List<SelectableItem> Items { get; private set; } = new();
    public Action<SelectableItem> OnSubmit { get; set; }
    public T GetSelectedItem<T>() where T : SelectableItem => Items[selectedIndex] as T;
    public void Submit()
    {
        if (!Enable) return;
        Items[selectedIndex].Submit();
    }

    public void Initialize()
    {
        selectedIndex = 0;
        ReselectCurrentItem();
    }

    public void ReselectCurrentItem()
    {
        if (Items.Count <= 0) return;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Items.Count - 1);
        foreach (var item in Items)
            item.Select(false);
        Items[selectedIndex].Select(true);
    }

    public void AddItem<T>(T item) where T : SelectableItem
    {
        Items.Add(item);
        item.transform.SetParent(container.transform, false);
        item.transform.localScale = Vector3.one;
        item.Initialize(() =>
        {
            Items[selectedIndex].Select(false);
            selectedIndex = Items.IndexOf(item);
            Items[selectedIndex].Select(true);
        },
        () =>
        {
            if (!Enable) return;
            OnSubmit?.Invoke(item);
        });
    }

    public void AddItems<T>(IEnumerable<T> items) where T : SelectableItem
    {
        foreach (var item in items)
            AddItem(item);
    }

    /// <summary>
    /// 指定したアイテムを管理下から除外する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="instanceDestroy"></param>
    public void RemoveItem<T>(T item, bool instanceDestroy = true) where T : SelectableItem
    {
        Items.Remove(item);
        if (instanceDestroy)
            Destroy(item.gameObject);
        ReselectCurrentItem();
    }

    /// <summary>
    /// 全てのアイテムを管理下から除外する
    /// </summary>
    /// <param name="instanceDestroy"></param>
    public void Clear(bool instanceDestroy = true)
    {
        if (instanceDestroy)
        {
            foreach (var item in Items)
                Destroy(item.gameObject);
        }
        Items.Clear();
    }

    public void Right()
    {
    }

    public void Left()
    {
    }

    public void Up() => Move(-1);

    public void Down() => Move(1);

    private void Move(int move)
    {
        Items[selectedIndex].Select(false);
        selectedIndex += move;
        if (selectedIndex < 0) selectedIndex += Items.Count;
        else if (selectedIndex >= Items.Count) selectedIndex %= Items.Count;
        Items[selectedIndex].Select(true);
    }

}
