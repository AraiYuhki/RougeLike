using UnityEditor;
using UnityEngine;

public class DungeonCreator : EditorWindow
{
    private static DungeonSetting setting = null;

    [MenuItem("Tools/�_���W��������")]
    public static void Open() => GetWindow<DungeonCreator>();

    public void OnGUI()
    {
        setting = EditorGUILayout.ObjectField("�ݒ�", setting, typeof(DungeonSetting), false) as DungeonSetting;
        if (GUILayout.Button("����"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        DungeonGenerator.Generate(setting);
    }
}
