using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DirectionalIndicator : MonoBehaviour
{
    Vector3 defaultPosition = new Vector3(0f, 0f, 1.05f);
    Sequence moveAnimation = null;
    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = defaultPosition - new Vector3(0f, 0f, 0.2f);
        InitializeAnimation();
    }

    private void InitializeAnimation()
    {
        moveAnimation = DOTween.Sequence();
        moveAnimation.Append(transform.DOLocalMove(defaultPosition + new Vector3(0f, 0f, 0.2f), 1f).SetEase(Ease.InOutQuad));
        moveAnimation.Append(transform.DOLocalMove(defaultPosition - new Vector3(0f, 0f, 0.2f), 1f).SetEase(Ease.InOutQuad));
        moveAnimation.SetLoops(-1);
        moveAnimation.Play();
        transform.DOLocalRotate(new Vector3(180f, -90f, -90f), 1.0f).SetEase(Ease.Linear).SetLoops(-1);
    }

    // Update is called once per frame
    void Update()
    {
        if (moveAnimation == null) InitializeAnimation();
    }

    private void OnDestroy()
    {
        moveAnimation?.Kill();
        moveAnimation = null;
    }
}
