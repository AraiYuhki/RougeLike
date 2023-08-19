using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private bool autoRotation = false;
    
    public ItemBase Data { get; set; }
    public int GemCount { get; set; } = 0;
    public bool IsGem => GemCount > 0;

    public Vector2Int Position { get; private set; }

    public void SetPosition(TileData tile)
    {
        Position = tile.Position;
        transform.localPosition = new Vector3(tile.Position.X, 0f, tile.Position.Y);
    }

    // Update is called once per frame
    void Update()
    {
        if (autoRotation)
            transform.rotation *= Quaternion.Euler(0f, 90f * Time.deltaTime, 0f);
    }
}
