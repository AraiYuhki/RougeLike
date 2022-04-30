using UnityEditor;
using UnityEngine;

public class DungeonCreator : EditorWindow
{
    private static DungeonSetting setting = null;

    [MenuItem("Tools/É_ÉìÉWÉáÉìê∂ê¨")]
    public static void Open() => GetWindow<DungeonCreator>();

    public void OnGUI()
    {
        setting = EditorGUILayout.ObjectField("ê›íË", setting, typeof(DungeonSetting), false) as DungeonSetting;
        if (GUILayout.Button("ê∂ê¨"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        DungeonGenerator.Generate(setting);
    }
}
