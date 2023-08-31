using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;

#if !EXCLUDE_UNITY_DEBUG_SHEET
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityDebugSheet.Runtime.Core.Scripts.DefaultImpl.Cells;
using Xeon.Master;

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
        AddButton("HP変更", clicked: () => player.Data.Hp = hpSlider.Value);
        AddSlider(staminaValue, 0f, 100f, "満腹度", valueChanged: value => staminaValue = value, valueTextFormat: "F1");
        AddButton("変更", clicked: () => player.Data.Stamina = staminaValue);
        AddInputField("ジェム", value: gemCount.ToString(), contentType: UnityEngine.UI.InputField.ContentType.IntegerNumber, valueChanged: value => { if (int.TryParse(value, out var newValue)) gemCount = newValue; });
        AddButton("追加", clicked: () => player.Data.Gems += gemCount);
        AddPicker(DB.Instance.MCard.All.Select(data => data.Name), selectedCardIndex, "カード", activeOptionChanged: index => selectedCardIndex = index);
        AddButton("追加", clicked: AddCard);
        AddButton("全てデッキに戻してシャッフル", clicked: () =>
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
        cardController.AddToDeck(DB.Instance.MCard.All[selectedCardIndex]);
    }
}
#endif
