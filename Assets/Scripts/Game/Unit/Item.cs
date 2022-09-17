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

    public void SetPosition(TileData tile) => transform.localPosition = new Vector3(tile.Position.x, 0f, tile.Position.y);

    // Update is called once per frame
    void Update()
    {
        if (autoRotation)
            transform.rotation *= Quaternion.Euler(0f, 0.5f, 0f);
    }
}
