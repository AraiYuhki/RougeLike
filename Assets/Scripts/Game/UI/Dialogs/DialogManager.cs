using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoSingleton<DialogManager>
{
    [SerializeField]
    private Image basePanel;

    [SerializeField]
    private List<DialogBase> dialogs = new List<DialogBase>();

    private List<DialogBase> dialogQueue = new List<DialogBase>();

    private Tween tween = null;

    public void Start()
    {
        basePanel.color = new Color(0f, 0f, 0f, 0f);
        basePanel.gameObject.SetActive(false);
    }

    public T Open<T>(Action onOpened = null) where T : DialogBase
    {
        var original = dialogs.FirstOrDefault(dialog => dialog.GetType() == typeof(T));
        if (original == null)
        {
            Debug.LogError($"{typeof(T)}'s dialog is not found");
            return null;
        }
        var dialog = Instantiate(original, transform) as T;
        if (dialogQueue.Any())
        {
            dialogQueue.First().Close();
        }
        else
        {
            tween?.Kill();
            basePanel.gameObject.SetActive(true);
            tween = basePanel.DOFade(0.5f, 0.2f);
            tween.OnComplete(() => tween = null);
        }
        dialogQueue.Insert(0, dialog); // キューの先頭に追加
        dialog.Open(onOpened);
        return dialog;
    }

    public void Close(DialogBase dialog)
    {
        var isFirst = dialogQueue.First() == dialog;
        if (isFirst)
        {
            dialogQueue.Remove(dialog);
            dialog.Close(() =>
            {
                Destroy(dialog.gameObject);
            });
            // キューにダイアログが残っているなら新しく開く
            if (dialogQueue.Any())
                dialogQueue.First()?.Open();
            if (!dialogQueue.Any())
            {
                tween?.Kill();
                tween = basePanel.DOFade(0f, 0.2f);
                tween.OnComplete(() =>
                {
                    tween = null;
                    basePanel.gameObject.SetActive(false);
                });
            }
            return;
        }
        // 現在開いているものではないのでクローズ処理をせずに削除
        dialogQueue.Remove(dialog);
        Destroy(dialog.gameObject);
    }

    public bool Controll()
    {
        if (!dialogQueue.Any()) return false;
        dialogQueue.First().Controll();
        return true;
    }
}
