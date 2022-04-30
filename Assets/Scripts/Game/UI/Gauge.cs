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
    private int max = 100;
    [SerializeField]
    private int value = 100;

    public float Progress => value / max;
    public int Max
    {
        get => max;
        set => max = value;
    }

    public void SetValue(int value)
    {
        this.value = value;
        if(label != null)
            label.text = $"{value}/{max}";
        foreground.fillAmount = Progress;
    }
}
