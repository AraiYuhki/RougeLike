using UnityEngine;
using DG.Tweening;
using System;

public class Unit : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public float DestAngle { get; set; }

    public bool EndRotation { get; protected set; }

    public void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
    }

    public virtual void Update()
    {
    }

    public void Move(int x, int z) => Move(new Vector2Int(x, z));
    public virtual void Move(Vector2Int move) => Move(move, null);

    public virtual void Attack(Vector3 targetPosition, TweenCallback onEndAttack = null)
    {
        var currentPosition = transform.localPosition;
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMove(targetPosition, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(transform.DOLocalMove(currentPosition, 0.2f).SetEase(Ease.OutCubic));
        if (onEndAttack != null) sequence.OnComplete(onEndAttack);
        sequence.SetAutoKill(true);
        sequence.Play();
    }

    public void MoveTo(Vector2Int destPosition)
    {
        var diff = destPosition - Position;
        SetPosition(destPosition);
        SetDestAngle(diff);
    }

    public void Move(Vector2Int move, TweenCallback onComplete = null)
    {
        var dest = Position + move;
        SetPosition(dest, onComplete);
        SetDestAngle(move);
    }


    public virtual void SetPosition(Vector2Int position, TweenCallback onComplete = null)
    {
        Position = position;
        var tween = transform.DOLocalMove(new Vector3(position.x, 0.5f, position.y), 0.2f).SetEase(Ease.OutCubic);
        if (onComplete != null) tween.onComplete = onComplete;
    }

    public virtual void SetDestAngle(Vector2Int move)
    {
        var destAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
        transform.DORotate(new Vector3(0f, destAngle, 0f), 0.1f).SetEase(Ease.OutCubic);
    }
}
