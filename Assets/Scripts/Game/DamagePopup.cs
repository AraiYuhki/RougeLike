using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;

    private Sequence tween;
    private IObjectPool<DamagePopup> pool;

    public void SetPool(IObjectPool<DamagePopup> pool) => this.pool = pool;

    public void Initialize(int value, Color color)
    {
        gameObject.SetActive(true);
        tween = DOTween.Sequence();
        label.text = value.ToString();
        label.color = color;
        var animator = new DOTweenTMPAnimator(label);
        label.alpha = 0f;
        for (var index = 0; index < animator.textInfo.characterCount; index++)
        {
            tween.Insert(index * 0.05f, animator.DOFadeChar(index, 1f, 0.2f));
            tween.Insert(index * 0.05f, animator.DOOffsetChar(index, Vector3.up * 100f, 0.2f).SetLoops(2, LoopType.Yoyo));
            tween.Insert(animator.textInfo.characterCount * 0.05f + 1.0f, animator.DOFadeChar(index, 0f, 0.2f));
        }
        
        tween.OnComplete(() =>
        {
            tween = null;
            pool.Release(this);
        });
    }

    private void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }
}
