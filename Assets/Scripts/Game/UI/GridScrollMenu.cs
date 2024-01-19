using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GridScrollMenu : MenuBase
{
    [SerializeField]
    private ScrollRect scrollView;
    [SerializeField]
    private GridLayoutGroup grid;

    [SerializeField]
    private RectOffset padding;
    [SerializeField]
    private Vector2 spacing;
    [SerializeField]
    private Vector2 cellSize = new Vector2(100, 100);
    [SerializeField, Tooltip("スクロールせずに表示される行・列数")]
    private Vector2Int viewCountInPage = new Vector2Int(3, 3);

    private Vector2Int selectedIndex = Vector2Int.zero;
    protected override int SelectedIndex { get => 0; set { } }

    private List<List<SelectableItem>> items = new();
    private int rowCount => items.Count;
    private int GetCurrentColumnCount() => items[selectedIndex.y].Count;

    public Action<SelectableItem> OnSubmit { get; set; }
    public T GetSelectedItem<T>() where T : SelectableItem => CurrentSelected as T;
    public SelectableItem CurrentSelected => items[selectedIndex.y][selectedIndex.x];

    public override void Submit()
    {
        if (!Enable) return;
        items[selectedIndex.y][selectedIndex.x].Submit();
    }

    public void Initialize()
    {
        selectedIndex = Vector2Int.zero;
        ReselectCurrentItem();
    }

    public override void ReselectCurrentItem(bool fixIndex = false)
    {
        if (items.Sum(row => row.Count) <= 0) return;
        if (fixIndex)
        {
            FixIndex();
            FixScroll();
        }
        CurrentSelected.Select(true);
    }

    public override void AddItem<T>(T item)
    {
        if (items.Count <= 0 || items.Last().Count >= viewCountInPage.x) items.Add(new());
        items.Last().Add(item);
        item.transform.parent = scrollView.content.transform;
        item.transform.localScale = Vector3.one;

        item.Initialize(() =>
        {
            CurrentSelected.Select(false);
            var indecies = new Vector2Int(-1, -1);
            foreach ((var row, var indexY) in items.Select((row, index) => (row, index)))
            {
                var indexX = row.IndexOf(item);
                if (indexX > 0)
                {
                    indecies.x = indexX;
                    indecies.y = indexY;
                    break;
                }
            }
            if (indecies.x < 0 || indecies.y < 0)
                return;
            selectedIndex = indecies;
            CurrentSelected.Select(true);
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

    public override void RemoveItem<T>(T item)
    {
        items.FirstOrDefault(row => row.Contains(item))?.Remove(item);
        ResetIndex();
        Destroy(item.gameObject);
    }

    private void ResetIndex()
    {
        var tmp = new List<List<SelectableItem>>();
        foreach (var item in items.SelectMany(row => row).ToList())
        {
            if (tmp.Count <= 0 || tmp.Count > viewCountInPage.x) tmp.Add(new());
            tmp.Last().Add(item);
        }
        items = tmp;
    }

    public override void Clear()
    {
        foreach (var item in items.SelectMany(row => row))
            Destroy(item.gameObject);
        items.Clear();
    }

    public override void Right() => Move(Vector2Int.right);
    public override void Left() => Move(Vector2Int.left);
    public override void Up() => Move(Vector2Int.down);
    public override void Down() => Move(Vector2Int.up);

    public void Move(Vector2Int move)
    {
        if (move.x == 0 && move.y == 0)
        {
            return;
        }
        CurrentSelected.Select(false);
        selectedIndex += move;
        FixIndex();
        FixScroll();
        CurrentSelected.Select(true);
    }

    protected override void FixIndex()
    {
        // 先にY軸インデックスを補正する
        if (selectedIndex.y < 0) selectedIndex.y += rowCount;
        else if (selectedIndex.y >= rowCount) selectedIndex.y -= rowCount;

        // 現在の行の列数を算出
        var currentLineColumns = GetCurrentColumnCount();
        // X軸のインデックスを補正する
        if (selectedIndex.x < 0) selectedIndex.x += currentLineColumns;
        else if (selectedIndex.x >= currentLineColumns) selectedIndex.x -= currentLineColumns;
    }

    private void FixScroll()
    {
        const float align = 0f;
        var targetRect = CurrentSelected.GetComponent<RectTransform>();
        var contentHeight = scrollView.content.rect.height;
        var viewPortHeight = scrollView.viewport.rect.height;
        // スクロールの必要なし
        if (contentHeight < viewPortHeight) return;

        // pivotによるズレをrec.yで補正
        var positionY = targetRect.localPosition.y + targetRect.rect.y;
        // ローカル座標が、contentHeightの上辺を0として負の値で格納されている
        var targetPosition = contentHeight + positionY + targetRect.rect.height * align;

        // 上端～下端合わせのための調整量
        var gap = viewPortHeight * align;
        scrollView.verticalNormalizedPosition = Mathf.Clamp01((targetPosition - gap) / (contentHeight - viewPortHeight));
    }

    private void OnEnable()
    {
        var rectTransform = GetComponent<RectTransform>();
        scrollView ??= GetComponent<ScrollRect>();

        if (grid == null) return;
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = viewCountInPage.x;
        grid.padding = padding;

        var size = (grid.cellSize + spacing) * viewCountInPage + new Vector2(padding.left + padding.right, padding.top + padding.bottom);
        rectTransform.sizeDelta = size;
        grid.CalculateLayoutInputHorizontal();
        grid.CalculateLayoutInputVertical();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        OnEnable();
    }
#endif
}
