using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TabGroups : MonoBehaviour
{
    [SerializeField]
    private List<ExToggle> tabs = new List<ExToggle>();
    [SerializeField]
    private List<GameObject> contents = new List<GameObject>();
    [SerializeField]
    private int selectIndex = 0;

    /// <summary>
    /// 変更前のタブインデックス、変更後のタブインデックス
    /// </summary>
    public Action OnChangeTab { get; set; }

    private bool isChanging = false;

    public int SelectIndex
    {
        get => selectIndex;
        set => UpdateView(value);
    }

    private void Start()
    {
        foreach((var tab, var index) in tabs.Select((tab, index) => (tab, index)))
        {
            tab.OnValueChanged = _ => 
            {
                UpdateView(index);
            };
        }
    }

    private void UpdateView(int newIndex)
    {
        if (isChanging) return;
        isChanging = true;
        if (newIndex < 0) newIndex += tabs.Count;
        else if (newIndex >= tabs.Count) newIndex -= tabs.Count;
        for (var index = 0; index < tabs.Count; index++)
        {
            tabs[index].Select(index == newIndex);
            contents[index].gameObject.SetActive(index == newIndex);
        }
        if (selectIndex == newIndex)
        {
            isChanging = false;
            return;
        }

        selectIndex = newIndex;
        OnChangeTab?.Invoke();
        isChanging = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateView(Mathf.Clamp(selectIndex, 0, tabs.Count - 1));
    }
#endif

}
