using DG.Tweening;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public enum MinimapMode
{
    Normal,
    Menu,
    Overlay,
}

public class Minimap : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private ItemManager itemManager;
    [SerializeField]
    private TrapManager trapManager;
    [SerializeField]
    private FloorManager floorManager;

    [SerializeField]
    private float tileSize = 30f;
    [SerializeField]
    private MinimapSymbol originalSymbol;
    [SerializeField]
    private Image tileLayer;
    [SerializeField]
    private Sprite playerSprite;
    [SerializeField]
    private Sprite stairSprite;
    [SerializeField]
    private Sprite unitSprite;
    [SerializeField]
    private Sprite itemSprite;
    [SerializeField]
    private Sprite trapSprite;

    [SerializeField]
    private Image playerIcon;

    [SerializeField]
    private Vector2 originalPosition = Vector2.zero;

    private Point prevPlayerPosition = new Point();
    private RectTransform rectTransform = null;
    private Texture2D texture;

    private ObjectPool<MinimapSymbol> symbolPool = null;
    private Dictionary<IPositionable, MinimapSymbol> activeSymbols = new();

    private Image stair = null;

    private FloorData floorData;
    private bool[,] visibleMap;

    private float halfTileSize => tileSize * 0.5f;

    private Sequence tween = null;

    public MinimapMode Mode { get; private set; } = MinimapMode.Normal;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        symbolPool = new ObjectPool<MinimapSymbol>(CreateSymbol,
            target => target.gameObject.SetActive(true),
            target => target.gameObject.SetActive(false),
            target => Destroy(target.gameObject),
            maxSize: 50);
        MinimapSymbol.TileSize = tileSize;
    }

    private MinimapSymbol CreateSymbol()
    {
        var instance = Instantiate(originalSymbol, tileLayer.transform);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private void AddSymbol(IPositionable target, Sprite sprite, Color color)
    {
        var symbol = symbolPool.Get();
        symbol.Setup(target, sprite, color);
        activeSymbols.Add(target, symbol);
    }

    public void AddItem(ItemData itemData) => AddSymbol(itemData, itemSprite, Color.green);
    public void AddTrap(TrapData trapData) => AddSymbol(trapData, trapSprite, Color.red);
    public void AddEnemy(Enemy enemy) => AddSymbol(enemy, itemSprite, Color.red);
    public void RemoveSymbol(IPositionable target)
    {
        if (activeSymbols.TryGetValue(target, out var symbol))
        {
            activeSymbols.Remove(target);
            symbolPool.Release(symbol);
        }
    }

    public void Clear()
    {
        foreach(var symbol in activeSymbols.Values) symbolPool.Release(symbol);
        symbolPool.Clear();
        activeSymbols.Clear();
    }

    public void Initialize(FloorData data)
    {
        floorData = data;
        var size = data.Size;
        visibleMap = new bool[size.X, size.Y];
        originalPosition = -halfTileSize * size + Vector2.one * halfTileSize;
        playerIcon.rectTransform.sizeDelta = Vector2.one * tileSize;

        Clear();

        if (texture != null) Destroy(texture);
        tileLayer.rectTransform.sizeDelta = size * tileSize;
        texture = new Texture2D(size.X, size.Y, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < size.Y; y++)
            for (var x = 0; x < size.X; x++)
                texture.SetPixel(x, y, Color.clear);
        texture.Apply();
        tileLayer.sprite = Sprite.Create(texture, new Rect(0, 0, size.X, size.Y), Vector2.zero);
        prevPlayerPosition = player.Position;
        if (stair == null)
            stair = CreateImage(tileLayer.transform, Color.green, stairSprite);
        var position = originalPosition;
        position.x += floorData.StairPosition.X * tileSize;
        position.y += floorData.StairPosition.Y * tileSize;
        stair.transform.localPosition = position;
        stair.gameObject.SetActive(false);
    }

    private void Update()
    {
        var position = -tileSize * player.Position.ToVector3();
        position -= originalPosition.ToVector3();
        if (player.Position != (Vector2Int)prevPlayerPosition)
        {
            SetVisibleMap(player.Position);

            stair.gameObject.SetActive(visibleMap[floorData.StairPosition.X, floorData.StairPosition.Y]);

            foreach ((var owner, var symbol) in activeSymbols)
            {
                symbol.UpdatePosition(originalPosition);
                symbol.SetVisible(CheckVisible(owner.Position));
            }

            prevPlayerPosition = player.Position;
        }
        tileLayer.transform.localPosition = position;
        playerIcon.transform.rotation = Quaternion.Euler(0f, 0f, -player.transform.localEulerAngles.y);
    }

    public void SetVisibleMap(Point position)
    {
        var currentTile = floorData.Map[position.X, position.Y];
        var changed = false;
        if (currentTile.IsRoom)
        {
            foreach (var tile in floorData.Map.ToArray().Where(tile => tile.IsRoom && tile.Id == currentTile.Id))
            {
                if (!VisibleTile(tile.Position))
                {
                    visibleMap[tile.Position.X, tile.Position.Y] = true;
                    changed = true;
                }
                // 部屋の周囲1マスも開く
                foreach (var pos in floorManager.GetAroundTilesAt(tile.Position).Select(x => x.Position).Where(pos => !VisibleTile(pos)))
                {
                    visibleMap[pos.X, pos.Y] = true;
                    changed = true;
                }
            }
        }
        foreach (var pos in floorManager.GetAroundTilesAt(position).Select(tile => tile.Position).Where(pos => !VisibleTile(pos)))
        {
            visibleMap[pos.X, pos.Y] = true;
            changed = true;
        }
        if (!VisibleTile(position))
        {
            visibleMap[position.X, position.Y] = true;
            changed = true;
        }
        if (!changed) return;

        foreach (var tile in floorData.Map.ToArray())
        {
            var visible = visibleMap[tile.Position.X, tile.Position.Y];
            if (!visible || tile.IsWall)
                continue;
            texture.SetPixel(tile.Position.X, tile.Position.Y, new Color(0f, 1f, 1f, 0.5f));
        }
        texture.Apply();
    }

    private bool VisibleTile(Point point) => visibleMap[point.X, point.Y];

    private bool CheckVisible(Vector2Int position)
    {
        var playerTile = floorData.Map[player.Position.x, player.Position.y];
        var targetTile = floorData.Map[position.x, position.y];
        var diff = player.Position - position;
        // 隣接しているもしくは同じ部屋に存在しているアイテムや敵だけをミニマップに表示
        var visible = Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1;
        if (playerTile.IsRoom && targetTile.IsRoom)
            visible = playerTile.Id == targetTile.Id;
        return visible;
    }

    private Image CreateImage(Transform layer, Color color, Sprite sprite)
    {
        var instance = new GameObject();
        instance.transform.parent = layer;
        instance.transform.localScale = Vector3.one;
        var image = instance.AddComponent<Image>();
        image.rectTransform.sizeDelta = Vector2.one * tileSize;
        image.color = color;
        image.sprite = sprite;
        return image;
    }

    public void SetMode(MinimapMode mode)
    {
        if (Mode == mode) return;
        tween?.Kill();
        tween = DOTween.Sequence();
        Mode = mode;
        switch(mode)
        {
            case MinimapMode.Normal:
                tween.Append(rectTransform.DOLocalMove(new Vector3(640f, 240f), 0.5f));
                tween.Join(rectTransform.DOSizeDelta(new Vector2(250f, 250f), 0.5f));
                break;
            case MinimapMode.Menu:
                tween.Append(rectTransform.DOLocalMove(new Vector3(115f, 85f), 0.5f));
                tween.Join(rectTransform.DOSizeDelta(new Vector2(910f, 410f), 0.5f));
                break;
            case MinimapMode.Overlay:
                tween.Append(rectTransform.DOLocalMove(Vector3.zero, 0.5f));
                tween.Join(rectTransform.DOSizeDelta(new Vector2(Screen.width - 10f, Screen.height - 10f), 0.5f));
                break;
        }
        tween.OnComplete(() => tween = null);
    }

    private void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }
}
