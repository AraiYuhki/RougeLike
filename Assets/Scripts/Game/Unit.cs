using UnityEngine;

public class Unit : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public Vector3 DestPosition { get; set; }
    public float DestAngle { get; set; }
    public float Rotation 
    {
        get => transform.rotation.y;
        set => transform.rotation = Quaternion.Euler(0, value, 0);
    }

    public void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
    }

    public virtual void Update()
    {
        transform.localPosition += (DestPosition - transform.localPosition) * 0.1f;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, DestAngle, 0), 0.1f);
    }

    public void Move(int x, int z) => Move(new Vector2Int(x, z));
    public virtual void Move(Vector2Int move)
    {
        var dest = Position + move;
        SetPosition(dest);
        SetDestAngle(move);
    }

    public void SetPosition(Vector2Int position)
    {
        Position = position;
        DestPosition = new Vector3(position.x, 0.5f, position.y);
    }

    public void SetDestAngle(Vector2Int move)
    {
        DestAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
    }
}
