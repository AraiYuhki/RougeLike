using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField]
    private DamagePopup original;

    private List<DamagePopup> popupList = new List<DamagePopup>();

    public void Create(Unit target, int value, Color color)
    {
        var popup = popupList.FirstOrDefault(popup => !popup.gameObject.activeSelf);
        if (popup == null)
        {
            popup = Instantiate(original, transform);
            popupList.Add(popup);
        }
        popup.transform.localPosition = target.transform.localPosition;
        popup.transform.rotation = Quaternion.identity;
        popup.transform.localScale = Vector3.one;
        popup.Initialize(value, color);
    }
}
