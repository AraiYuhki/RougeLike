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
    [SerializeField]
    private bool percentView = false;

    public float Progress => (float)value / max;
    public int Max
    {
        get => max;
        set => max = value;
    }

    public void SetValue(int value)
    {
        this.value = value;
        if(label != null)
            label.text = percentView ? string.Format("{0:0}%", Progress * 100) : $"{value}/{max}";
        foreground.fillAmount = Progress;
    }
}
