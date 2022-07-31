using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text weaponLabel;
    [SerializeField]
    private TMP_Text shieldLabel;
    [SerializeField]
    private TMP_Text currentExpLabel;
    [SerializeField]
    private TMP_Text nextExpLabel;
    [SerializeField]
    private TMP_Text strLabel;
    [SerializeField]
    private TMP_Text atkLabel;
    [SerializeField]
    private TMP_Text defLabel;
    [SerializeField]
    private CanvasGroup group;

    private Player player = null;
    private Tweener animationTween = null;
    private PlayerData data => player.Data;
    public CanvasGroup Group => group;
    public void Initialize(Player player) => this.player = player;
    
    public void Open(TweenCallback onComplete = null)
    {
        if (animationTween != null)
        {
            animationTween.Complete();
            animationTween = null;
        }
        gameObject.SetActive(true);
        group.alpha = 0;
        animationTween = group.DOFade(1f, 0.2f).OnComplete(() =>
        {
            animationTween = null;
            onComplete?.Invoke();
        });
    }
    
    public void Close(TweenCallback onComplete = null)
    {
        if (animationTween != null)
        {
            animationTween.Complete();
            animationTween = null;
        }
        group.alpha = 1f;
        animationTween = group.DOFade(0f, 0.2f).OnComplete(() =>
        {
            animationTween = null;
            onComplete?.Invoke();
            gameObject.SetActive(false);
        });
    }

    private void Update()
    {
        if (data == null) return;
        weaponLabel.text = data.EquipmentWeapon == null ? "ïêäÌ: ëféË" : $"ïêäÌ: {data.EquipmentWeapon.Name}";
        shieldLabel.text = data.EquipmentShield == null ? "èÇ: Ç»Çµ" : $"èÇ: {data.EquipmentShield.Name}";
        currentExpLabel.text = $"EXP: {data.TotalExp}";
        nextExpLabel.text = $"NEXT: {data.NextLevelExp}";
        strLabel.text = $"STR: {data.Atk}/8";
        atkLabel.text = $"ATK: {data.WeaponPower}";
        defLabel.text = $"DEF: {data.Def}";
    }
}
