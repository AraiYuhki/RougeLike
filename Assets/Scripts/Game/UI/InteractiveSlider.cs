using TMPro;
using UnityEngine;

public class InteractiveSlider : ControllableItem
{
    [SerializeField]
    private TMP_Text titleLabel;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private UnityEngine.UI.Slider slider;
    [SerializeField]
    private float minValue = 0f;
    [SerializeField]
    private float maxValue = 100f;
    [SerializeField]
    private bool isInterger = true;

    private bool isValueChanging = false;
    public string Label
    {
        get => titleLabel.text;
        set => titleLabel.text = value;
    }
    public float Value
    {
        get => slider.value;
        set => slider.value = value;
    }

    public int IntValue => Mathf.FloorToInt(slider.value);

    private void Awake()
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.onValueChanged.AddListener(value =>
        {
            if (isValueChanging) return;
            isValueChanging = true;
            input.text = (isInterger ? Mathf.FloorToInt(value) : value).ToString();
            isValueChanging = false;
        });

        input.text = slider.value.ToString();
        input.onValueChanged.AddListener(value =>
        {
            if (isValueChanging) return;
            isValueChanging = true;
            if (float.TryParse(value, out var result))
                slider.value = isInterger ? Mathf.FloorToInt(result) : result;
            isValueChanging = false;
        });
    }

    public override void Right()
    {
        slider.value++;
    }

    public override void Left()
    {
        slider.value--;
    }
}
