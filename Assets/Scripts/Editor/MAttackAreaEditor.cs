using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class MAttackAreaEditor : EditorWindow
{
    [MenuItem("Tools/攻撃範囲エディタ")]
    public static void Open()
    {
        var window = GetWindow<MAttackAreaEditor>();
        window.titleContent = new GUIContent("攻撃範囲エディタ");
    }

    private MAttackArea areaMaster = null;
    private MAttack attackMaster = null;
    private ReorderableList list = null;

    private AttackAreaInfo editingData = null;
    
    private int size = 3;
    private Vector2Int center;
    private bool[,] flags = null;

    public void OnDestroy()
    {
        EditorUtility.SetDirty(areaMaster);
        AssetDatabase.SaveAssetIfDirty(areaMaster);
    }

    public void OnGUI()
    {
        areaMaster = EditorGUILayout.ObjectField(areaMaster, typeof(MAttackArea), false) as MAttackArea;
        attackMaster = EditorGUILayout.ObjectField(attackMaster, typeof(MAttack), false) as MAttack;
        if (areaMaster == null || attackMaster == null)
        {
            editingData = null;
            return;
        }
        var halfSize = position.width / 2 - 5;
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginDisabledGroup(editingData != null);
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfSize)))
            {
                InitializeList();
                list.DoLayoutList();
            }
            EditorGUI.EndDisabledGroup();
            if (editingData == null) return;
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfSize)))
            {
                DrawEditor();
            }
        }
    }

    private void DrawData(Rect rect, int index, bool isActive, bool isForcused)
    {
        var data = areaMaster.All[index];
        rect.width -= 100;
        EditorGUI.LabelField(rect, $"ID:{data.Id}: {data.Memo}");
        rect.x += rect.width;
        rect.width = 100;
        if(GUI.Button(rect, "編集"))
        {
            InitializeEditData(data.Clone());
        }
    }

    private void InitializeEditData(AttackAreaInfo original)
    {
        editingData = original;
        var attackData = attackMaster.GetByGroupId(editingData.AttackGroupId) ?? new List<AttackInfo>();
        if (attackData.Count <= 0)
        {
            size = 3;
            center = Vector2Int.one;
        }
        else
        {
            size = attackData.Max(data => Mathf.Max(Mathf.Abs(data.Offset.x), Mathf.Abs(data.Offset.y))) * 2 + 1;
            center = Vector2Int.one * (size / 2);
        }

        flags = new bool[size, size];
        foreach (var data in attackData)
        {
            var offset = data.Offset + center;
            flags[offset.x, offset.y] = true;
        }
    }

    private void InitializeList()
    {
        if (list == null)
        {
            list = new ReorderableList(areaMaster.All, typeof(AttackAreaInfo));
            list.drawElementCallback = DrawData;
            list.onAddCallback = AddArea;
            list.onRemoveCallback = RemoveArea;
            list.drawHeaderCallback = rect => GUI.Label(rect, "データ");
        }
    }

    private void DrawEditor()
    {
        if (editingData == null)
        {
            EditorGUILayout.LabelField("編集対象を選択してください");
            return;
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("適用"))
            {
                ApplyData();
                return;
            }
            if (GUILayout.Button("破棄"))
            {
                editingData = null;
                return;
            }
        }
        EditorGUI.BeginChangeCheck();
        var tmpSize = EditorGUILayout.IntSlider("サイズ", size, 3, 99);
        if (EditorGUI.EndChangeCheck()) UpdateData(tmpSize);
        editingData.Memo = EditorGUILayout.TextField("メモ", editingData.Memo);
        EditorGUILayout.LabelField("オリジナル");
        if (flags == null) flags = new bool[size, size];
        using (new EditorGUILayout.VerticalScope())
        {
            for (var y = 0; y < size; y++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < size; x++)
                    {
                        EditorGUI.BeginDisabledGroup(x == center.x && y == center.y);
                        flags[x, y] = EditorGUILayout.Toggle(flags[x, y], GUILayout.Width(20));
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }
        }
    }

    private void AddArea(ReorderableList list)
    {
        var newData = new AttackAreaInfo(
            areaMaster.All.Max(area => area.Id) + 1,
            areaMaster.All.Max(area => area.AttackGroupId) + 1
            );
        areaMaster.All.Add(newData);
        areaMaster.Reset();
        EditorUtility.SetDirty(areaMaster);
        AssetDatabase.SaveAssetIfDirty(areaMaster);
    }

    private void RemoveArea(ReorderableList list)
    {
        var target = areaMaster.All[list.index];
        areaMaster.All.Remove(target);
        areaMaster.Reset();
        attackMaster.ReplaceByGroupId(target.AttackGroupId, new List<AttackInfo>());
        EditorUtility.SetDirty(areaMaster);
        EditorUtility.SetDirty(attackMaster);
        AssetDatabase.SaveAssetIfDirty(areaMaster);
        AssetDatabase.SaveAssetIfDirty(attackMaster);
        
    }

    private void UpdateData(int tmpSize)
    {
        size = tmpSize % 2 == 1 ? tmpSize : size;
        center.x =
        center.y = Mathf.CeilToInt(size / 2);
        var tmpData = new bool[size, size];
        for (var x = 0; x < flags.GetLength(0); x++)
        {
            for (var y = 0; y < flags.GetLength(1); y++)
            {
                if (x >= tmpData.GetLength(0) || y >= tmpData.GetLength(1)) continue;
                if (x >= flags.GetLength(0) || y >= flags.GetLength(1)) continue;
                tmpData[x, y] = flags[x, y];
            }
        }
        tmpData[center.x, center.y] = false;
        flags = tmpData;
    }

    private void ApplyData()
    {
        var groupId = editingData.AttackGroupId;
        var data = new List<AttackInfo>();
        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                if (!flags[x, y]) continue;
                var offset = new Vector2Int(x, y) - center;
                data.Add(new AttackInfo(groupId, offset, 1));
            }
        }
        var original = areaMaster.GetById(editingData.Id);
        original.Memo = editingData.Memo;
        attackMaster.ReplaceByGroupId(groupId, data);
        editingData.SetData(data);
        editingData = null;
        EditorUtility.SetDirty(areaMaster);
        EditorUtility.SetDirty(attackMaster);
        AssetDatabase.SaveAssetIfDirty(areaMaster);
        AssetDatabase.SaveAssetIfDirty(attackMaster);
    }
}
