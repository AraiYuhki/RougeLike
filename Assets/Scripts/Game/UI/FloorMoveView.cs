using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class FloorMoveView : MonoBehaviour
{
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private TMP_Text currentFloorLabel;
    [SerializeField]
    private TMP_Text nextFloorLabel;
    [SerializeField]
    private bool isTower;

    private Sequence tween = null;

    public void Start()
    {
        canvas.enabled = false;
    }

    public void StartFadeOut(int currentFloor, int nextFloor, bool isTower, Action onFadeComplete, Action onComplete)
    {
        tween.Complete();
        canvasGroup.alpha = 0f;
        canvas.enabled = true;
        currentFloorLabel.text = isTower ? $"{currentFloor}F" : $"B{currentFloor}F";
        nextFloorLabel.text = isTower ? $"{nextFloor}F" : $"B{nextFloor}F";
        currentFloorLabel.transform.localPosition = Vector3.zero;
        currentFloorLabel.color = new Color(1f, 1f, 1f, 1f);
        nextFloorLabel.transform.localPosition = Vector3.up * (isTower ? 100f : -100f);
        nextFloorLabel.color = new Color(1f, 1f, 1f, 0.3f);
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(1f, 0.5f));
        tween.AppendCallback(() => onFadeComplete?.Invoke());
        tween.AppendInterval(0.5f);
        tween.Append(currentFloorLabel.transform.DOLocalMoveY(isTower ? -100f : 100f, 0.2f));
        tween.Join(nextFloorLabel.transform.DOLocalMoveY(0f, 0.2f));
        tween.Join(currentFloorLabel.DOFade(0.3f, 0.2f));
        tween.Join(nextFloorLabel.DOFade(1f, 0.2f));
        tween.OnComplete(() =>
        {
            onComplete?.Invoke();
            tween = null;
        });
    }

    public void StartFadeIn(Action onComplete = null)
    {
        tween?.Complete();
        canvasGroup.alpha = 1f;
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(0f, 0.5f));
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
            canvas.enabled = false;
        });
    }
}
