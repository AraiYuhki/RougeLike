using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class DungeonEditor : EditorWindow
{
    private class EditableFloorInfo
    {
        public FloorInfo FloorInfo { get; set; }
        public bool Foldout { get; set; }

        public EditableFloorInfo(FloorInfo floorInfo)
        {
            FloorInfo = floorInfo;
        }

        public EditableFloorInfo(int dungeonId)
        {
            FloorInfo = new FloorInfo(dungeonId);
        }
    }

    enum Mode
    {
        Select,
        Edit
    }
    private Mode mode = Mode.Select;
    private DungeonInfo dungeonInfo;
    private List<EditableFloorInfo> floorInfoList;

    private List<int> enemySpawnGroupIdList = new();

    private ReorderableList floorListView = null;

    private Vector2 dungeonScrollPosition = Vector2.zero;
    private Vector2 floorScrollPosition = Vector2.zero;

    [MenuItem("Tools/ダンジョン設定ツール")]
    public static void Open() => GetWindow<DungeonEditor>("ダンジョン設定");

    public void OnGUI()
    {
        if (mode == Mode.Select)
            DungeonSelectMode();
        else
            DungeonEditMode();
    }

    private void Load(DungeonInfo target)
    {
        dungeonInfo = target.Clone();
        floorInfoList = DB.Instance.MFloor.GetByDungeonId(dungeonInfo.Id).Select(info => new EditableFloorInfo(info.Clone())).ToList();
        floorListView = new ReorderableList(floorInfoList, typeof(EditableFloorInfo));
        floorListView.onAddCallback = view =>
        {
            floorInfoList.Add(new EditableFloorInfo(dungeonInfo.Id));
        };
        floorListView.onRemoveCallback = view =>
        {
            var deleteTargets = view.selectedIndices.Select(index => floorInfoList[index]).ToList();
            foreach (var target in deleteTargets)
            {
                floorInfoList.Remove(target);
            }
        };
        floorListView.drawElementCallback = DrawFloorEditor;
        floorListView.elementHeightCallback = index => floorInfoList[index].Foldout ? 10 * EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;

        enemySpawnGroupIdList = DB.Instance.MFloorEnemySpawn.All.GroupBy(info => info.GroupId).Select(group => group.Key).ToList();
    }

    private void Create()
    {
        dungeonInfo = new DungeonInfo(DB.Instance.MDungeon.All.Max(info => info.Id) + 1);
        floorInfoList = new();
    }

    private void Clear()
    {
        dungeonInfo = null;
        floorInfoList.Clear();
    }

    private void Save()
    {
        var db = DB.Instance;
        if (db.MDungeon.All.Any(info => info.Id == dungeonInfo.Id))
            db.MDungeon.GetById(dungeonInfo.Id).Apply(dungeonInfo);
        else
            db.MDungeon.All.Add(dungeonInfo);
        db.MFloor.RemoveByDungeonId(dungeonInfo.Id);
        db.MFloor.AddData(floorInfoList.Select(data => data.FloorInfo).ToList());
        EditorUtility.SetDirty(db.MDungeon);
        EditorUtility.SetDirty(db.MFloor);
        AssetDatabase.SaveAssets();
    }

    private void RemoveDungeon(int dungeonId)
    {
        var db = DB.Instance;
        db.MDungeon.RemoveById(dungeonId);
        db.MFloor.RemoveByDungeonId(dungeonId);
        EditorUtility.SetDirty(db.MDungeon);
        EditorUtility.SetDirty(db.MFloor);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// ダンジョン設定モード描画
    /// </summary>
    private void DungeonSelectMode()
    {
        if (GUILayout.Button("新規作成", GUILayout.Width(200f)))
        {
            Create();
            mode = Mode.Edit;
        }
        var dungeonList = DB.Instance.MDungeon;
        using (var scrollView = new EditorGUILayout.ScrollViewScope(dungeonScrollPosition))
        {
            foreach (var dungeon in dungeonList.All)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"ID:{dungeon.Id} {dungeon.Name}");
                    if (GUILayout.Button("読み込み"))
                    {
                        Load(dungeon);
                        mode = Mode.Edit;
                    }
                    if (GUILayout.Button("削除"))
                    {
                        if (EditorUtility.DisplayDialog("確認", $"{dungeon.Name}を削除してもよろしいですか？", "OK", "キャンセル"))
                            RemoveDungeon(dungeon.Id);
                    }
                }
            }

            dungeonScrollPosition = scrollView.scrollPosition;
        }
    }

    /// <summary>
    /// ダンジョン編集モード描画
    /// </summary>
    private void DungeonEditMode()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
        {
            if (GUILayout.Button("保存"))
            {
                Save();
            }
            if (GUILayout.Button("破棄して再読み込み"))
            {
                Load(dungeonInfo);
                return;
            }
            if (GUILayout.Button("戻る"))
            {
                Clear();
                mode = Mode.Select;
                return;
            }
        }
        dungeonInfo.SetName(EditorGUILayout.DelayedTextField("ダンジョン名", dungeonInfo.Name));
        dungeonInfo.SetIsTower(EditorGUILayout.Toggle("登る", dungeonInfo.IsTower));

        using (var scrollView = new EditorGUILayout.ScrollViewScope(floorScrollPosition))
        {
            floorListView.DoLayoutList();
            floorScrollPosition = scrollView.scrollPosition;
        }
    }

    private void DrawFloorEditor(Rect rect, int index, bool isActive, bool isFocused)
    {
        var floor = floorInfoList[index].FloorInfo;
        rect.height = EditorGUIUtility.singleLineHeight;

        var startIndex = 1;
        for (var count = 0; count < index; count++)
        {
            startIndex += floorInfoList[count].FloorInfo.SameSettingCount + 1;
        }
        var label = CreateFloorLabel(startIndex, floor.SameSettingCount, dungeonInfo.IsTower);
        floorInfoList[index].Foldout = EditorGUI.Foldout(rect, floorInfoList[index].Foldout, label);
        if (!floorInfoList[index].Foldout)
        {
            return;
        }

        var originalX = rect.x;
        var height = EditorGUIUtility.singleLineHeight + 5f;
        rect.y += height;
        floor.SetSize(EditorGUI.Vector2IntField(rect, "フロアサイズ", floor.Size));
        rect.y += height * 2f;

        var originalWidth = rect.width;
        rect.width = rect.width * 0.5f - 20f;

        floor.SetDeletePathProbability(EditorGUI.Slider(rect, "通路の削除率", floor.DeletePathProbability, 0f, 1f));
        rect.x += rect.width + 10f;

        floor.SetMaxRoomCount(EditorGUI.IntSlider(rect, "最大部屋数", floor.MaxRoomCount, 1, 999));

        rect.x = originalX;
        rect.y += height;

        EditorGUIUtility.labelWidth = 200;

        floor.SetInitialSpawnEnemyCount(EditorGUI.IntSlider(rect, "侵入時に出現する敵の数", floor.InitialSpawnEnemyCount, 0, 30));
        rect.x += rect.width + 10f;

        floor.SetSpawnEnemyIntervalTurn(EditorGUI.IntSlider(rect, "敵が出現する間隔(ターン数)", floor.SpawnEnemyIntervalTurn, 1, 1000));
        rect.x = originalX;
        rect.y += height;

        EditorGUIUtility.labelWidth = 150f;

        floor.SetFloorMaterial(EditorGUI.ObjectField(rect, "床素材", floor.FloorMaterial, typeof(Material), false) as Material);
        rect.x += rect.width + 10f;
        floor.SetWallMaterial(EditorGUI.ObjectField(rect, "壁素材", floor.WallMaterial, typeof(Material), false) as Material);

        rect.x = originalX;
        rect.width = originalWidth;
        rect.y += height;

        EditorGUI.BeginChangeCheck();
        var selectIndex = enemySpawnGroupIdList.IndexOf(floor.EnemySpawnGroupId);
        selectIndex = EditorGUI.Popup(rect, "敵出現パターン", selectIndex, enemySpawnGroupIdList.Select(group => $"ID:{group}").ToArray());
        if (EditorGUI.EndChangeCheck())
            floor.SetEnemySpawnGroupId(enemySpawnGroupIdList[selectIndex]);
        rect.y += height;
        floor.SetSameSettingCount(EditorGUI.IntField(rect, "同じ設定が続く階数", floor.SameSettingCount));
    }

    /// <summary>
    /// 階層表示用のラベルを作成
    /// </summary>
    /// <param name="start"></param>
    /// <param name="sameSettingCount"></param>
    /// <param name="isTower"></param>
    /// <returns></returns>
    private string CreateFloorLabel(int start, int sameSettingCount, bool isTower)
    {
        if (sameSettingCount == 0)
            return isTower ? $"{start}F" : $"B{start}F";
        return isTower ? $"{start}F～{start + sameSettingCount}F" : $"B{start}F～B{start + sameSettingCount}F";
    }
}
