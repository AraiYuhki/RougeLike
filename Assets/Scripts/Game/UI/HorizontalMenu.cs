using System;
using System.Collections.Generic;

public class HorizontalMenu : MenuBase
{
    public Action<SelectableItem> OnSubmit { get; set; }
    protected override int SelectedIndex { get; set; } = 0;

    private int columnCount => Items.Count;

    public void Initialize()
    {
        SelectedIndex = 0;
    }

    public void AddItems<T>(IEnumerable<T> items) where T : SelectableItem
    {
        foreach (var item in items) AddItem(item);
    }

    public override void AddItem<T>(T item)
    {
        base.AddItem(item);
        item.Initialize(() =>
        {
            Items[SelectedIndex].Select(false);
            var index = Items.IndexOf(item);
            SelectedIndex = index % columnCount;
            Items[SelectedIndex].Select(true);
        }, () =>
        {
            if (!Enable) return;
            OnSubmit?.Invoke(item);
        });
    }
    public override void Right() => Move(1);

    public override void Left() => Move(-1);
    public override void Up() { }
    public override void Down() { }

    private void Move(int value)
    {
        Items[SelectedIndex].Select(false);
        SelectedIndex += value;
        FixIndex();
        Items[SelectedIndex].Select(true);
    }

    protected override void FixIndex()
    {
        if (SelectedIndex >= columnCount)
            SelectedIndex -= columnCount;
        if (SelectedIndex < 0)
            SelectedIndex += columnCount;
    }

}
