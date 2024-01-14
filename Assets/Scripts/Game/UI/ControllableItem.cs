using UnityEngine;

public abstract class ControllableItem : InteractiveItem
{
    [SerializeField]
    protected float duration;

    protected override float fadeDuration => duration;
}
