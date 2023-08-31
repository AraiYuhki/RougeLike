using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Xeon.Master;

public class MAttackAreaEditor : EditorWindow
{
    [MenuItem("Tools/MAttackAreaEditor")]
    public static void Open() => GetWindow<MAttackAreaEditor>();

    private Xeon.Master.MAttackArea master = null;
    private ReorderableList list = null;

    private AttackAreaInfo editingData = null;
    
    private int size = 3;
    private Vector2Int center;
    private bool[,] flags = null;

    public void OnDestroy()
    {
        EditorUtility.SetDirty(master);
        AssetDatabase.SaveAssetIfDirty(master);
    }

    public void OnGUI()
    {
        master = EditorGUILayout.ObjectField(master, typeof(Xeon.Master.MAttackArea), false) as Xeon.Master.MAttackArea;
        if (master == null) return;

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
        var data = master.All[index];
        
        rect.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(rect, $"ID:{index}");
        rect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(rect, "サイズ", data.MaxSize.ToString());
        rect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(rect, "中心点", data.Center.ToString());
        rect.y += EditorGUIUtility.singleLineHeight * 1.5f;
        var tmpRect = rect;
        tmpRect.width = 10;
        tmpRect.height = 10;
        for (var y = 0; y < data.MaxSize; y++)
        {
            for (var x = 0; x < data.MaxSize; x++)
            {
                var color = Color.gray;
                if (data.Center.x == x && data.Center.y == y)
                    color = Color.black;
                else if (data.Data.Any(d => d.Offset.x == x && d.Offset.y == y))
                    color = Color.cyan;
                EditorGUI.DrawRect(tmpRect, color);
                tmpRect.x += tmpRect.width + 2;
            }
            tmpRect.x = rect.x;
            tmpRect.y += tmpRect.height + 2;
        }
        rect.y = tmpRect.y + EditorGUIUtility.singleLineHeight;
        if (GUI.Button(rect, "編集"))
            InitializeEditData(data);
    }

    private float CalculateHeight(int index)
    {
        var data = master.All[index];
        return EditorGUIUtility.singleLineHeight * (data.MaxSize * 2 + 2);
    }

    private void InitializeEditData(AttackAreaInfo original)
    {
        editingData = original;
        size = original.MaxSize;
        center = original.Center;

        flags = new bool[size, size];
        foreach (var data in editingData.Data)
            flags[data.Offset.x, data.Offset.y] = true;
    }

    private void InitializeList()
    {
        if (list == null)
        {
            list = new ReorderableList(master.All, typeof(AttackAreaInfo));
            list.drawElementCallback = DrawData;
            list.elementHeightCallback = CalculateHeight;
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
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Vector2IntField("中心", center);
        EditorGUI.EndDisabledGroup();
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
                data.Add(new AttackInfo(groupId, new Vector2Int(x, y), 1));
            }
        }
        editingData.SetData(data);
        editingData = null;
        EditorUtility.SetDirty(master);
        AssetDatabase.SaveAssetIfDirty(master);
    }
}
