using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

[ExecuteInEditMode]
public class NoticeGroup : MonoBehaviour
{
    [SerializeField]
    private NoticeItem originalItem = null;
    [SerializeField]
    private Vector3 originalPosition = new Vector3(250f, -140f, 0f);
    [SerializeField]
    private float space = 10f;
    [SerializeField]
    private int maxStackCount = 10;

    private List<NoticeItem> items = new List<NoticeItem>();

    public void Add(string message, Color? color = null)
    {
        var destColor = color.HasValue ? color.Value : Color.black;
        destColor.a = 0.5f;
        var item = Instantiate(originalItem, transform);
        item.transform.SetSiblingIndex(0);
        item.SetMessage(message, destColor, () => items.Remove(item));
        items.Insert(0, item);
        if(items.Count > maxStackCount)
        {
            var lastItem = items.Last();
            lastItem.ForceDestroy();
            items.Remove(lastItem);
        }
    }

    private void Update()
    {
        var position = originalPosition;
        foreach(var item in items)
        {
            var rectTransform = item.GetComponent<RectTransform>();
            item.SetDestPosition(position);
            position.y += rectTransform.rect.height + space;
        }
    }
}
