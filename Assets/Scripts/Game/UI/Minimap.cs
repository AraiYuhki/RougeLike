using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    private Texture2D texture;

    private List<Image> enemies = new List<Image>();
    private List<Image> items = new List<Image>();

    private Player player => ServiceLocator.Instance.GameController.Player;
    private EnemyManager enemyManager => ServiceLocator.Instance.EnemyManager;
    private ItemManager itemManager => ServiceLocator.Instance.ItemManager;

    [SerializeField]
    private Vector2 originalPosition = Vector2.zero;

    private float halfTileSize => tileSize * 0.5f;

    public void Initialize(FloorData data)
    {
        var size = data.Size;
        originalPosition = -halfTileSize * size + Vector2.one * halfTileSize;

        foreach (var enemy in enemies) Destroy(enemy);
        enemies.Clear();
        foreach (var item in items) Destroy(item);
        items.Clear();

        if (texture != null) Destroy(texture);
        tileLayer.rectTransform.sizeDelta = size * tileSize;
        texture = new Texture2D(size.X, size.Y, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (var y = 0; y < size.Y; y++)
        {
            for (var x = 0; x < size.X; x++)
            {
                texture.SetPixel(x, y, data.Map[x, y].IsWall ? Color.clear : new Color(0f, 1f, 1f, 0.5f));
            }
        }
        texture.Apply();
        tileLayer.sprite = Sprite.Create(texture, new Rect(0, 0, size.X, size.Y), Vector2.zero);
    }

    private void Update()
    {
        var position = -tileSize * player.Position.ToVector3();
        position -= originalPosition.ToVector3();
        tileLayer.transform.localPosition = position;
        playerIcon.transform.rotation = Quaternion.Euler(0f, 0f, -player.transform.localEulerAngles.y);
        UpdateEnemies();
        UpdateItems();
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
            this.enemies[index].gameObject.SetActive(true);
            var position = this.enemies[index].transform.localPosition;
            position.x = enemy.Position.x * tileSize + originalPosition.x;
            position.y = enemy.Position.y * tileSize + originalPosition.y;
            this.enemies[index].transform.localPosition = position;
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
            this.items[index].gameObject.SetActive(true);
            var position = this.items[index].transform.localPosition;
            position.x = item.Position.x * tileSize + originalPosition.x;
            position.y = item.Position.y * tileSize + originalPosition.y;
            this.items[index].transform.localPosition = position;
        }
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
}
