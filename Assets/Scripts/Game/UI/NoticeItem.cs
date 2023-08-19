using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class NoticeItem : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private Image background;
    [SerializeField]
    private TMP_Text message;
    [SerializeField]
    private float moveRate = 4f;

    private Vector3 destPosition = Vector3.zero;
    private Sequence tween = null;

    public void Awake()
    {
        destPosition = transform.localPosition;
        canvasGroup.alpha = 0f;
    }

    public void SetMessage(string text, Color backgroundColor, Action onDestroy = null)
    {
        background.color = backgroundColor;
        message.text = text;
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(1f, 0.2f));
        tween.AppendInterval(3.0f);
        tween.Append(canvasGroup.DOFade(0f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            onDestroy?.Invoke();
            Destroy(gameObject);
        });
    }

    public void ForceDestroy()
    {
        tween?.Kill();
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(0f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            Destroy(gameObject);
        });
    }

    public void SetPosition(Vector3 position)
    {
        transform.localPosition =
        destPosition = position;
    }

    public void SetDestPosition(Vector3 position)
    {
        destPosition = position;
    }

    private void Update()
    {
        if (Mathf.Abs((transform.localPosition - destPosition).magnitude) < 0.01f)
            transform.localPosition = destPosition;
        else
        {
            transform.localPosition += (destPosition - transform.localPosition) * Time.deltaTime * moveRate;
        }
    }
}
