using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExToggle : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private Image image;
    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField]
    private Color selectedColor = new Color(0f, 1f, 1f, 0.5f);

    public bool IsOn => toggle.isOn;


    public Action<bool> OnValueChanged { get; set; }
    public void SetToggleGroup(ToggleGroup group) => toggle.group = group;

    private Tween tween = null;

    public void Start()
    {
        toggle.onValueChanged.AddListener(flag =>
        {
            OnValueChanged?.Invoke(IsOn);
            var destColor = IsOn ? selectedColor : normalColor;
            tween?.Kill();
            tween = image.DOColor(destColor, 0.2f);
            tween.OnComplete(() => tween = null);
        });
    }

    private void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }

    public void Select(bool flag)
    {
        toggle.isOn = flag;
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        image.color = IsOn ? selectedColor : normalColor;
#endif
    }
}
