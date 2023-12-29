using System.Collections.Generic;
using UnityEngine;

public abstract class MenuBase : MonoBehaviour
{
    [SerializeField]
    protected Transform container;

    public List<SelectableItem> Items { get; set; } = new List<SelectableItem>();
    public abstract void Up();
    public abstract void Down();
    public abstract void Right();
    public abstract void Left();

    public bool Enable { get; set; }
    protected abstract int SelectedIndex { get; set; }

    public virtual void Submit()
    {
        if (!Enable) return;
        Items[SelectedIndex].Submit();
    }

    public virtual void ReselectCurrentItem(bool fixIndex = false)
    {
        if (Items.Count <= 0) return;
        Items[SelectedIndex].Select(true);
    }

    public virtual void AddItem<T>(T item) where T: SelectableItem
    {
        Items.Add(item);
        item.transform.parent = container;
        item.transform.localScale = Vector3.one;
    }

    public virtual void RemoveItem<T>(T item) where T : SelectableItem
    {
        Items.Remove(item);
        Destroy(item.gameObject);
    }

    public void Clear()
    {
        foreach (var item in Items)
            Destroy(item.gameObject);
        Items.Clear();
    }

    protected virtual void FixIndex()
    {
    }
}
