using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image)), RequireComponent(typeof(RectTransform))]
public class MinimapSymbol : MonoBehaviour
{
    public static float TileSize { get; set; }

    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Image image;
    
    private IPositionable owner;

    public void Setup(IPositionable owner, Sprite sprite, Color color)
    {
        image.sprite = sprite;
        image.color = color;
        rectTransform.sizeDelta = Vector2.one * TileSize;
        this.owner = owner;
    }

    public void UpdatePosition(Vector2 originalPosition)
    {
        var position = originalPosition;
        position.x += owner.Position.x * TileSize;
        position.y += owner.Position.y * TileSize;
        transform.localPosition = new Vector3(position.x, position.y, 0);
    }

    public void SetVisible(bool visible) => gameObject.SetActive(visible);
#if UNITY_EDITOR
    public void OnValidate()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (image == null)
            image = GetComponent<Image>();
    }
#endif
}
