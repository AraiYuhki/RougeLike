using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Drawing.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleMenuItem : SelectableItem
{
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private Transform animationTarget;
    [SerializeField]
    private float fromX;
    [SerializeField]
    private float destX;
    [SerializeField]
    private float duration = 0.5f;

    public void Ready()
    {
        var position = animationTarget.localPosition;
        position.x = fromX;
        animationTarget.localPosition = position;
        canvasGroup.alpha = 0f;
    }

    public async UniTask PlayAnimation()
    {
        Ready();
        await DOTween.Sequence()
            .Append(animationTarget.DOLocalMoveX(destX, duration))
            .Join(canvasGroup.DOFade(1f, duration))
            .ToUniTask(cancellationToken: destroyCancellationToken);
    }
#if UNITY_EDITOR

    private Tween debugTweener = null;
    private bool isPreviewing = false;
    private void Preview()
    {
        debugTweener?.Complete();

        Ready();

        debugTweener = DOTween.Sequence()
            .Append(animationTarget.DOLocalMoveX(destX, duration))
            .Join(canvasGroup.DOFade(1f, duration))
            .OnComplete(() =>
            { 
                debugTweener = null;
                EditorApplication.update -= Repaint;
                isPreviewing = false;
            });
        EditorApplication.update -= Repaint;
        EditorApplication.update += Repaint;
        isPreviewing = true;
    }
    private void StopPreview()
    {
        debugTweener?.Complete();
        debugTweener = null;
    }
    private void Repaint()
    {
        debugTweener?.ManualUpdate(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
    }

    [CustomEditor(typeof(TitleMenuItem))]
    private class TitleMenuItemInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var instance = target as TitleMenuItem;
            if (instance.isPreviewing)
            {
                if(GUILayout.Button("プレビュー停止"))
                    instance.StopPreview();
                return;
            }
            if (GUILayout.Button("プレビュー"))
            {
                instance.Preview();
            }
        }
    }
#endif
}
