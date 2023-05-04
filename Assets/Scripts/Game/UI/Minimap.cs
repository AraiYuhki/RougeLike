using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    private float tileSize = 30f;
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

    private List<Image> enemies = new List<Image>();
    private List<Image> items = new List<Image>();

    private FloorData floorData;
    private bool[,] visibleMap;

    private Player player => ServiceLocator.Instance.GameController.Player;
    private EnemyManager enemyManager => ServiceLocator.Instance.EnemyManager;
    private ItemManager itemManager => ServiceLocator.Instance.ItemManager;

    private float halfTileSize => tileSize * 0.5f;

    private Sequence tween = null;

    public MinimapMode Mode { get; private set; } = MinimapMode.Normal;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(FloorData data)
    {
        floorData = data;
        var size = data.Size;
        visibleMap = new bool[size.X, size.Y];
        originalPosition = -halfTileSize * size + Vector2.one * halfTileSize;
        playerIcon.rectTransform.sizeDelta = Vector2.one * tileSize;

        foreach (var enemy in enemies) Destroy(enemy);
        enemies.Clear();
        foreach (var item in items) Destroy(item);
        items.Clear();

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
    }

    private void Update()
    {
        var position = -tileSize * player.Position.ToVector3();
        position -= originalPosition.ToVector3();
        if (player.Position != (Vector2Int)prevPlayerPosition)
        {
            SetVisibleMap(player.Position);
            prevPlayerPosition = player.Position;
        }
        tileLayer.transform.localPosition = position;
        playerIcon.transform.rotation = Quaternion.Euler(0f, 0f, -player.transform.localEulerAngles.y);
        UpdateEnemies();
        UpdateItems();
    }

    private void SetVisibleMap(Point position)
    {
        var currentTile = floorData.Map[position.X, position.Y];
        if (currentTile.IsRoom)
        {
            foreach(var tile in floorData.Map.ToArray().Where(tile => tile.IsRoom && tile.Id == currentTile.Id))
                visibleMap[tile.Position.X, tile.Position.Y] = true;
        }
        visibleMap[position.X, position.Y] = true;
        visibleMap[position.X + 1, position.Y] = true;
        visibleMap[position.X - 1, position.Y] = true;
        visibleMap[position.X, position.Y + 1] = true;
        visibleMap[position.X, position.Y - 1] = true;
        visibleMap[position.X + 1, position.Y + 1] = true;
        visibleMap[position.X + 1, position.Y - 1] = true;
        visibleMap[position.X - 1, position.Y + 1] = true;
        visibleMap[position.X - 1, position.Y - 1] = true;

        foreach(var tile in floorData.Map.ToArray())
        {
            var visible = visibleMap[tile.Position.X, tile.Position.Y];
            if (!visible || tile.IsWall)
                continue;
            texture.SetPixel(tile.Position.X, tile.Position.Y, new Color(0f, 1f, 1f, 0.5f));
        }
        texture.Apply();
    }

    private void UpdateEnemies()
    {
        var enemies = enemyManager.Enemies;
        while (this.enemies.Count < enemies.Count)
            this.enemies.Add(CreateImage(tileLayer.transform, Color.red, unitSprite));

        foreach (var image in this.enemies)
            image.gameObject.SetActive(false);

        foreach ((var enemy, var index) in enemies.Select((enemy, index) => (enemy, index)))
        {
            var enemyTile = floorData.Map[enemy.Position.x, enemy.Position.y];
            var position = this.enemies[index].transform.localPosition;
            position.x = enemy.Position.x * tileSize + originalPosition.x;
            position.y = enemy.Position.y * tileSize + originalPosition.y;
            this.enemies[index].transform.localPosition = position;
            this.enemies[index].gameObject.SetActive(CheckVisible(enemy.Position));
        }
    }

    private void UpdateItems()
    {
        var items = itemManager.ItemList;
        while(this.items.Count < items.Count)
            this.items.Add(CreateImage(tileLayer.transform, Color.green, unitSprite));

        foreach (var image in this.items)
            image.gameObject.SetActive(false);

        foreach ((var item, var index) in items.Select((item, index) => (item, index)))
        {
            var itemTile = floorData.Map[item.Position.x, item.Position.y];
            var position = this.items[index].transform.localPosition;
            position.x = item.Position.x * tileSize + originalPosition.x;
            position.y = item.Position.y * tileSize + originalPosition.y;
            this.items[index].transform.localPosition = position;
            this.items[index].gameObject.SetActive(CheckVisible(item.Position));
        }
    }

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
                tween.Append(rectTransform.DOLocalMove(new Vector3(480f, 145f), 0.5f));
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
}
