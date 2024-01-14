using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfigDialog : DialogBase
{
    [SerializeField]
    private List<InteractiveItem> items = new List<InteractiveItem>();
    [SerializeField]
    private InteractiveSlider bgmSlider;
    [SerializeField]
    private InteractiveSlider seSlider;
    [SerializeField]
    private InteractiveSlider voiceSlider;

    [SerializeField]
    private SelectableItem exitButton;

    public override DialogType Type => DialogType.Config;
    public event Action OnExit;
    private ConfigData data = null;

    protected override void Initialize()
    {
        DataBank.Instance.Load<ConfigData>(ConfigData.SaveKey);
        data = DataBank.Instance.Get<ConfigData>(ConfigData.SaveKey) ?? new ConfigData();

        bgmSlider.Value = data.BGMVolume;
        seSlider.Value = data.SEVolume;
        voiceSlider.Value = data.VoiceVolume;
        exitButton.OnClick.AddListener(() => OnExit?.Invoke());
    }

    protected override void OnOpened() => UpdateView();

    public override void Controll()
    {
        if (InputUtility.Up.IsTrigger())
            Up();
        else if (InputUtility.Down.IsTrigger())
            Down();
        if (InputUtility.Left.IsRepeat())
            Left();
        else if (InputUtility.Right.IsRepeat())
            Right();
        else if (InputUtility.Submit.IsTrigger())
            Submit();
    }

    private void UpdateView()
    {
        foreach ((var item, var index) in items.Select((item, index) => (item, index)))
            item.Select(index == currentSelected);
    }

    public override void Up()
    {
        currentSelected--;
        if (currentSelected < 0) currentSelected += items.Count;
        UpdateView();
    }

    public override void Down()
    {
        currentSelected++;
        if (currentSelected >= items.Count) currentSelected -= items.Count;
        UpdateView();
    }

    public override void Right()
    {
        items[currentSelected].Right();
    }

    public override void Left()
    {
        items[currentSelected].Left();
    }

    public override void Submit()
    {
        DataBank.Instance.Store(ConfigData.SaveKey, new ConfigData(bgmSlider.IntValue, seSlider.IntValue, voiceSlider.IntValue));
        DataBank.Instance.Save(ConfigData.SaveKey);
        items[currentSelected].Submit();
    }
}
