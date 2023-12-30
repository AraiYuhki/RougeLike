using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    protected FloorManager floorManager;
    protected NoticeGroup noticeGroup;

    protected Material wallMaterial;
    protected Material floorMaterial;
   
    public Vector2Int Position { get; set; }

    protected virtual void Awake() => gameObject.SetActive(false);
    public virtual void Setup(FloorManager floorManager, NoticeGroup noticeGroup)
    {
        this.floorManager = floorManager;
        this.noticeGroup = noticeGroup;
    }

    public virtual void SetMaterials(Material wallMaterial, Material floorMaterial)
    {
        this.wallMaterial = wallMaterial;
        this.floorMaterial = floorMaterial;
    }

    public abstract TrapType Type { get; }

    protected CancellationToken CreateLinkedToken(CancellationToken playerToken)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(), playerToken).Token;
    }

    public virtual UniTask ExecuteAsync(Unit executer, CancellationToken token = default)
    {
        return UniTask.CompletedTask;
    }

    public void SetPosition(Vector2Int newPosition)
    {
        Position = newPosition;
        var y = Type == TrapType.Pitfall ? 0f : 0.5f;
        transform.localPosition = new Vector3(Position.x, y, Position.y);
    }
}
