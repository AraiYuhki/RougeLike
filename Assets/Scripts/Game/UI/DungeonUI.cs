using System.Collections;
using System.Collections.Generic;
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
    private Player player;

    private PlayerData Data => player.Data;

    // Update is called once per frame
    void Update()
    {
        if (Data == null) return;
        
        hpGauge.Max = Data.MaxHP;
        staminaGauge.Max = Data.MaxStamina;
        levelLabel.text = Data.Lv.ToString();
        expGauge.Max = 10;

        hpGauge.SetValue(Data.Hp);
        staminaGauge.SetValue((int)Data.Stamina);
        expGauge.SetValue(Data.TotalExp);
    }
}
