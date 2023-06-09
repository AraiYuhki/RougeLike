using UnityEditor;
using UnityEngine;

public class DungeonCreator : EditorWindow
{
    private static DungeonSetting setting = null;

    [MenuItem("Tools/ダンジョン生成")]
    public static void Open() => GetWindow<DungeonCreator>();

    public void OnGUI()
    {
        setting = EditorGUILayout.ObjectField("設定", setting, typeof(DungeonSetting), false) as DungeonSetting;
        if (GUILayout.Button("生成"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        DungeonGenerator.Generate(setting);
    }
}
