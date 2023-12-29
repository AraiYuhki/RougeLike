using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private bool autoRotation = false;
    
    public int GemCount { get; set; } = 0;
    public bool IsGem => GemCount > 0;

    private Tween tween = null;

    public Vector2Int Position { get; private set; }

    public void SetPosition(TileData tile)
    {
        Position = tile.Position;
        transform.localPosition = new Vector3(tile.Position.X, 0f, tile.Position.Y);
    }

    public void JumpTo()
    {
        tween?.Kill();
        tween = transform.DOLocalJump(transform.localPosition, 1, 1, 0.4f);
        tween.OnComplete(() => tween = null);
    }

    // Update is called once per frame
    private void Update()
    {
        if (autoRotation)
            transform.rotation *= Quaternion.Euler(0f, 90f * Time.deltaTime, 0f);
    }

    private void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }
}
