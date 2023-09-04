using UnityEditor;
using UnityEngine;

public class RotationTester : EditorWindow
{
    private class Point
    {
        public Vector2Int position;
        public bool flag;
    }
    [MenuItem("Tools/RotationTester")]
    public static void Open()
    {
        GetWindow<RotationTester>();
    }

    private int angle = 0;

    private bool[,] original = new bool[3,3];
    private int size = 3;
    public void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        var tmpSize = EditorGUILayout.IntSlider("サイズ", size, 3, 999);
        EditorGUI.BeginDisabledGroup(true);
        var center = Mathf.CeilToInt(tmpSize / 2);
        EditorGUILayout.IntField("中心", center);
        EditorGUI.EndDisabledGroup();
        if (tmpSize % 2 == 1) size = tmpSize;
        if (EditorGUI.EndChangeCheck())
        {
            CreateOriginalData();
        }
        EditorGUILayout.LabelField("オリジナル");
        using (new EditorGUILayout.VerticalScope())
        {
            for (var y = 0; y < size; y++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < size; x++)
                    {
                        EditorGUI.BeginDisabledGroup(x == center && y == center);
                        original[x, y] = EditorGUILayout.Toggle(original[x, y], GUILayout.Width(20));
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }
        }
        angle = EditorGUILayout.IntSlider("角度", angle, 0, 359);
        EditorGUILayout.LabelField("回転後");
        EditorGUI.BeginDisabledGroup(true);
        using (new EditorGUILayout.VerticalScope())
        {
            for (var y = 0; y < size; y++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < size; x++)
                    {
                        (var rotatedX, var rotatedY) = AttackAreaInfo.GetRotatedOffset(angle, new Vector2Int(x - center, y - center));
                        EditorGUILayout.Toggle(original[rotatedX + center, rotatedY + center], GUILayout.Width(20));
                    }
                }
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateOriginalData()
    {
        original = new bool[size, size];
    }
}
