using UnityEditor;
using UnityEngine;
public class FloorCreator : EditorWindow
{
    private static int width = 20;
    private static int height = 20;
    private static int maxRoom = 3;
    private static bool isTower = false;

    [MenuItem("Tools/�t���A����")]
    public static void Open()
    {
        GetWindow<FloorCreator>();
    }

    public void OnGUI()
    {
        width = EditorGUILayout.IntSlider("��", width, 20, 100);
        height = EditorGUILayout.IntSlider("���s", height, 20, 100);
        maxRoom = EditorGUILayout.IntSlider("�ő啔����", maxRoom, 3, 100);
        isTower = EditorGUILayout.Toggle("��", isTower);

        if (GUILayout.Button("����"))
        {
            var floor = FindObjectOfType<FloorManager>();
            while (floor.transform.childCount > 0) DestroyImmediate(floor.transform.GetChild(0).gameObject);
            floor.Create(width, height, maxRoom, isTower);
        }
        if (GUILayout.Button("�N���A"))
        {
            var floor = FindObjectOfType<FloorManager>();
            while (floor.transform.childCount > 0) DestroyImmediate(floor.transform.GetChild(0).gameObject);
        }
    }
}
