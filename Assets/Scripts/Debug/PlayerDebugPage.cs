using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;

#if !EXCLUDE_UNITY_DEBUG_SHEET
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityDebugSheet.Runtime.Core.Scripts.DefaultImpl.Cells;

public class PlayerDebugPage : DefaultDebugPageBase
{
    protected override string Title => "Player debug menu";

    private Player player;
    
    private float staminaValue = 100f;
    
    private int selectedCardIndex = 0;

    private int hpSliderIndex = -1;
    private SliderCellModel hpSlider = null;

    private int gemCount = 100;

    protected override void Start()
    {
        player = FindObjectOfType<Player>();
        base.Start();
    }

    public override IEnumerator Initialize()
    {
        hpSlider = new SliderCellModel(true, 1, 10);
        hpSlider.ValueTextFormat = "F1";
        hpSlider.CellTexts.Text = "HP";
        hpSliderIndex = AddSlider(hpSlider);
        AddButton("HP�ύX", clicked: () => player.Data.Hp = hpSlider.Value);
        AddSlider(staminaValue, 0f, 100f, "�����x", valueChanged: value => staminaValue = value, valueTextFormat: "F1");
        AddButton("�ύX", clicked: () => player.Data.Stamina = staminaValue);
        AddInputField("�W�F��", value: gemCount.ToString(), contentType: UnityEngine.UI.InputField.ContentType.IntegerNumber, valueChanged: value => { if (int.TryParse(value, out var newValue)) gemCount = newValue; });
        AddButton("�ǉ�", clicked: () => player.Data.Gems += gemCount);
        AddPicker(DataBase.Instance.GetTable<MCard>().Data.Select(data => data.Name), selectedCardIndex, "�J�[�h", activeOptionChanged: index => selectedCardIndex = index);
        AddButton("�ǉ�", clicked: AddCard);
        AddButton("�S�ăf�b�L�ɖ߂��ăV���b�t��", clicked: () =>
        {
            var cardController = FindObjectOfType<CardController>();
            cardController.Shuffle(true);
        });
        yield break;
    }

    private void Update()
    {
        if (player == null) return;
        hpSlider.MaxValue = player.MaxHp;
        RefreshDataAt(hpSliderIndex);
    }

    private void AddCard()
    {
        var cardController = FindObjectOfType<CardController>();
        cardController.AddToDeck(DataBase.Instance.GetTable<MCard>().Data[selectedCardIndex]);
    }
}
#endif
