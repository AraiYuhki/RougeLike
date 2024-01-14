using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[Serializable]
public class TrapData : IPositionable
{
    [SerializeField]
    private Vector2Int position;
    [SerializeField]
    private bool isVisible;
    [SerializeField]
    private TrapInfo master;

    private Trap owner;

    public Trap Owner => owner;
    public bool IsVisible => isVisible;

    public TrapType Type => master.Type;
    public Vector2Int Position => position;
    public GameObject gameObject => owner.gameObject;
    public TrapInfo Master => master;

    public TrapData() { }
    public TrapData(Trap owner, TileData tile, TrapInfo master, FloorManager floorManager, NoticeGroup notice)
    {
        this.owner = owner;
        this.position = tile.Position;
        this.master = master.Clone();
        owner.Setup(this.master, floorManager, notice);
        owner.SetPosition(position);
    }

    public async UniTask ExecuteAsync(Unit executer, CancellationToken token)
    {
        isVisible = true;
        await owner.ExecuteAsync(executer, token);
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
        owner.SetVisible(visible);
    }
}
