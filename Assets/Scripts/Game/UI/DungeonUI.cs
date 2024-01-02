using TMPro;
using UnityEngine;

public class DungeonUI : MonoBehaviour
{
    [SerializeField]
    private Gauge hpGauge;
    [SerializeField]
    private Gauge staminaGauge;
    [SerializeField]
    private TMP_Text walletLabel;
    [SerializeField]
    private Player player;
    [SerializeField]
    private Minimap minimap;

    private PlayerData Data => player.PlayerData;

    public Minimap Minimap => minimap;

    // Update is called once per frame
    void Update()
    {
        if (Data == null) return;

        hpGauge.Max = player.MaxHp;
        staminaGauge.Max = (int)Data.MaxStamina;
        walletLabel.text = $"{Data.Gems}G";

        hpGauge.SetValue(Mathf.FloorToInt(Data.Hp));
        staminaGauge.SetValue((int)Data.Stamina);
    }
}
