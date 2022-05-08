using UnityEditor;
using UnityEngine;
public class FloorCreator : EditorWindow
{
    private static int width = 20;
    private static int height = 20;
    private static int maxRoom = 3;
    private static bool isTower = false;

    [MenuItem("Tools/フロア生成")]
    public static void Open()
    {
        GetWindow<FloorCreator>();
    }

    public void OnGUI()
    {
        width = EditorGUILayout.IntSlider("幅", width, 20, 100);
        height = EditorGUILayout.IntSlider("奥行", height, 20, 100);
        maxRoom = EditorGUILayout.IntSlider("最大部屋数", maxRoom, 3, 100);
        isTower = EditorGUILayout.Toggle("塔", isTower);

        if (GUILayout.Button("生成"))
        {
            var floor = FindObjectOfType<FloorManager>();
            while (floor.transform.childCount > 0) DestroyImmediate(floor.transform.GetChild(0).gameObject);
            floor.Create(width, height, maxRoom, isTower);
        }
        if (GUILayout.Button("クリア"))
        {
            var floor = FindObjectOfType<FloorManager>();
            while (floor.transform.childCount > 0) DestroyImmediate(floor.transform.GetChild(0).gameObject);
        }
    }
}
