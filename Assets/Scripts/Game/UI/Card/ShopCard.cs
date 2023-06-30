using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCard : MonoBehaviour
{
    [SerializeField]
    private Image illust;
    [SerializeField]
    private TMP_Text label;
    [SerializeField]
    private Button button;

    private CardData data;

    public void SetData(CardData data, Action onClick = null)
    {
        this.data = data;
        label.text = data.Name;

        button.onClick.AddListener(() => onClick?.Invoke());
    }

}
