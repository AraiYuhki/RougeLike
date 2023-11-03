using UnityEngine;

public enum TrapType
{
    Mine,       // �n��
    HugeMine,   // �傫�Ȓn��
    PoisonTrap, // �ŕH
    BearTrap,   // �g���o�T�~
    Pitfall,    // ���Ƃ���
}

public abstract class Trap : MonoBehaviour
{
    protected FloorManager floorManager;
    protected NoticeGroup noticeGroup;
   
    public Vector2Int Position { get; set; }

    public virtual void Setup(FloorManager floorManager, NoticeGroup noticeGroup)
    {
        this.floorManager = floorManager;
        this.noticeGroup = noticeGroup;
    }
    public abstract TrapType Type { get; }
    public bool IsVisible { get; set; }

    public virtual void Execute(Unit executer)
    {
        IsVisible = true;
    }

    public void SetPosition(Vector2Int newPosition)
    {
        Position = newPosition;
        var y = Type == TrapType.Pitfall ? 0f : 0.5f;
        transform.localPosition = new Vector3(Position.x, y, Position.y);
    }
}
