using TMPro;
using UnityEngine;

public class DungeonUI : MonoBehaviour
{
    [SerializeField]
    private Gauge hpGauge;
    [SerializeField]
    private Gauge staminaGauge;
    [SerializeField]
    private Gauge expGauge;
    [SerializeField]
    private TMP_Text levelLabel;
    [SerializeField]
    private TMP_Text walletLabel;
    [SerializeField]
    private Player player;
    [SerializeField]
    private Minimap minimap;

    private PlayerData Data => player.Data;

    public Minimap Minimap => minimap;

    // Update is called once per frame
    void Update()
    {
        if (Data == null) return;

        hpGauge.Max = player.MaxHp;
        staminaGauge.Max = (int)Data.MaxStamina;
        levelLabel.text = Data.Lv.ToString();
        expGauge.Max = Data.NextLevelExp;
        walletLabel.text = $"{Data.Gems}G";

        hpGauge.SetValue(Mathf.FloorToInt(Data.Hp));
        staminaGauge.SetValue((int)Data.Stamina);
        expGauge.SetValue(Data.CurrentLevelExp);
    }
}
