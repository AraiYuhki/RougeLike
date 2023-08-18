using UnityEditor;
using UnityEngine;

public class DungeonCreator : EditorWindow
{
    private static DungeonSetting setting = null;
    private int floorCount = 1;
    private FloorSetting floorSetting = null;
    [MenuItem("Tools/ダンジョン生成")]
    public static void Open() => GetWindow<DungeonCreator>();

    public void OnGUI()
    {
        setting = EditorGUILayout.ObjectField("設定", setting, typeof(DungeonSetting), false) as DungeonSetting;
        if (GUILayout.Button("生成"))
        {
            Generate();
        }
        floorCount = EditorGUILayout.IntField("floorCount", floorCount);
        if (GUILayout.Button("フロア設定取得"))
        {
            floorSetting = setting.GetFloor(floorCount);
        }
        if (floorSetting == null) return;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField(floorSetting.MaxRoomCount.ToString());
        EditorGUILayout.LabelField(floorSetting.Size.ToString());
        EditorGUI.EndDisabledGroup();
    }

    private void Generate()
    {
        DungeonGenerator.Generate(setting);
    }
}
