using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    [SerializeField]
    private Image foreground = null;
    [SerializeField]
    private TMP_Text label;
    [SerializeField]
    private float max = 100f;
    [SerializeField]
    private float value = 100f;
    [SerializeField]
    private bool percentView = false;

    public float Progress => value / max;
    public float Max
    {
        get => max;
        set => max = value;
    }

    public void SetData(LimitedParam param, bool floor = false)
    {
        if (floor)
        {
            max = (int)param.Max;
            value = Mathf.Clamp(Mathf.FloorToInt(param.Value), 0, max);
        }
        else
        {
            max = param.Max;
            value = Mathf.Clamp(param.Value, 0, max);
        }
        UpdateView();
    }

    public void SetValue(float value)
    {
        this.value = Mathf.Clamp(value, 0, max);
        UpdateView();
    }

    private void UpdateView()
    {
        if (label != null)
            label.text = percentView ? string.Format("{0:0}%", Progress * 100) : $"{value}/{max}";
        if (foreground != null)
            foreground.fillAmount = Progress;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        value = Mathf.Clamp(value, 0, max);
        UpdateView();
    }
#endif
}
