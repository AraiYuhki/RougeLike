using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridScrollMenu : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollView;
    [SerializeField]
    private Transform container;
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private GridLayoutGroup grid;

    private int columnCount = 0;
    private int rowCount = 0;

    private int rowCountInPage = 0;

    private int topRowIndex = 0;
    private int bottomRowIndex = 0;

    private Vector2Int selectedIndex = Vector2Int.zero;

    private int SelectedIndex => columnCount * selectedIndex.y + selectedIndex.x;

    public List<SelectableItem> Items { get; private set; } = new List<SelectableItem>();
    public bool Enable { get; set; }
    public Action<SelectableItem> OnSubmit { get; set; }
    public T GetSelectedItem<T>() where T : SelectableItem => Items[SelectedIndex] as T;
    public void Submit()
    {
        if (!Enable) return;
        Items[SelectedIndex].Submit();
    }
    public void Initialize()
    {
        selectedIndex = Vector2Int.zero;
        ReselectCurrentItem();
    }

    public void ReselectCurrentItem(bool fixIndex = false)
    {
        if (Items.Count <= 0) return;
        if (fixIndex)
        {
            FixIndex();
            FixScroll();
        }
        Items[SelectedIndex].Select(true);
    }

    private void Awake()
    {
        columnCount = Mathf.FloorToInt(rectTransform.rect.width / grid.cellSize.x);
        rowCountInPage = Mathf.FloorToInt(rectTransform.rect.height / grid.cellSize.y) - 1;
        bottomRowIndex = rowCountInPage;
    }

    public void AddItem<T>(T item) where T : SelectableItem
    {
        Items.Add(item);
        item.transform.parent = container.transform;
        item.transform.localScale = Vector3.one;

        item.Initialize(() =>
        {
            Items[SelectedIndex].Select(false);
            var index = Items.IndexOf(item);
            selectedIndex.x = index % columnCount;
            selectedIndex.y = index / columnCount;
            Items[SelectedIndex].Select(true);
        },
        () =>
        {
            if (!Enable) return;
            OnSubmit?.Invoke(item);
        });
        if (Items.Count > 0)
        {
            rowCount = Mathf.CeilToInt(Items.Count / columnCount);
            if (Items.Count % columnCount > 0)
                rowCount++;
        }
    }

    public void RemoveItem<T>(T item) where T : SelectableItem
    {
        Items.Remove(item);
        Destroy(item.gameObject);
        if (Items.Count > 0)
        {
            rowCount = Mathf.CeilToInt(Items.Count / columnCount);
            if (Items.Count % columnCount > 0)
                rowCount++;
        }
        else
            rowCount = 0;
    }

    public void Clear()
    {
        foreach (var item in Items)
            Destroy(item.gameObject);
        Items.Clear();
    }

    public void Right() => Move(Vector2Int.right);
    public void Left() => Move(Vector2Int.left);
    public void Up() => Move(Vector2Int.down);
    public void Down() => Move(Vector2Int.up);

    public void Move(Vector2Int move)
    {
        if (move.x == 0 && move.y == 0)
        {
            return;
        }
        Items[SelectedIndex].Select(false);
        selectedIndex += move;
        FixIndex();
        FixScroll();
        Items[SelectedIndex].Select(true);
    }

    private void FixIndex()
    {
        // 先にY軸インデックスを補正する
        if (selectedIndex.y < 0) selectedIndex.y += rowCount;
        else if (selectedIndex.y >= rowCount) selectedIndex.y -= rowCount;

        // 現在の行の列数を算出
        var currentLineColumns = columnCount;
        var lastIndex = (selectedIndex.y + 1) * columnCount - 1; // 現在の行の末尾のインデックスを算出
        // 現在の行末のインデックスが要素数より大きい場合、列数が足りていないので、差の分だけ減らす
        if (lastIndex >= Items.Count)
        {
            var diff = lastIndex - Items.Count;
            currentLineColumns = columnCount - (diff + 1);
            selectedIndex.x = Mathf.Clamp(selectedIndex.x, 0, currentLineColumns);
        }
        // X軸のインデックスを補正する
        if (selectedIndex.x < 0) selectedIndex.x += currentLineColumns;
        else if (selectedIndex.x >= currentLineColumns) selectedIndex.x -= currentLineColumns;
    }

    private void FixScroll()
    {
        if (selectedIndex.y <= topRowIndex)
        {
            topRowIndex = selectedIndex.y;
            bottomRowIndex = topRowIndex + rowCountInPage;
        }
        else if (selectedIndex.y >= bottomRowIndex)
        {
            bottomRowIndex = selectedIndex.y;
            topRowIndex = bottomRowIndex - rowCountInPage;
        }
        if (bottomRowIndex == rowCount - 1)
            scrollView.verticalNormalizedPosition = 0f;
        else if (topRowIndex == 0)
            scrollView.verticalNormalizedPosition = 1f;
        else
        {
            var value = 1f / (rowCount - 2)* topRowIndex;
            scrollView.verticalNormalizedPosition = value == 0f ? 1f : 1 - value;
        }
    }
}
