using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TabGroups : MonoBehaviour
{
    [SerializeField]
    private List<SelectableItem> tabs = new List<SelectableItem>();
    [SerializeField]
    private List<GameObject> contents = new List<GameObject>();
    [SerializeField]
    private int selectIndex = 0;

    public Action OnChangeTab { get; set; }

    public int SelectIndex => selectIndex;

    private void Start()
    {
        foreach((var tab, var index) in tabs.Select((tab, index) => (tab, index)))
        {
            tab.Initialize(null, () => UpdateView(index));
        }
    }

    private void UpdateView(int newIndex)
    {
        for (var index = 0; index < tabs.Count; index++)
        {
            tabs[index].Select(index == newIndex);
            contents[index].gameObject.SetActive(index == newIndex);
        }
        if (selectIndex == newIndex) return;

        selectIndex = newIndex;
        OnChangeTab?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateView(Mathf.Clamp(selectIndex, 0, tabs.Count - 1));
    }
#endif

}
