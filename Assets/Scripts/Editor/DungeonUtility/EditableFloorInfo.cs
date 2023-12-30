using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class DungeonEditor
{
    private class EditableFloorInfo
    {
        public FloorInfo FloorInfo { get; set; }
        private bool foldout;

        private bool foldoutEnemyInfo = false;

        public EditableFloorInfo(FloorInfo floorInfo)
        {
            FloorInfo = floorInfo;
        }

        public EditableFloorInfo(int dungeonId)
        {
            FloorInfo = new FloorInfo(dungeonId);
        }

        public float GetContentHeight()
        {
            if (!foldout) return EditorGUIUtility.singleLineHeight;
            var height = EditorGUIUtility.singleLineHeight + 5f;
            var result = height * 11f;
            if (!foldoutEnemyInfo) return result;
            return result + height * DB.Instance.MFloorEnemySpawn.GetByGroupId(FloorInfo.EnemySpawnGroupId).Count;
        }

        public void DrawFloorEditor(Rect rect, string floorLabel, List<int> enemySpawnGroupIdList, List<int> trapSettingGroupIdList)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foldout = EditorGUI.Foldout(rect, foldout, floorLabel);
            if (!foldout)
            {
                return;
            }

            var originalX = rect.x;
            var height = EditorGUIUtility.singleLineHeight + 5f;
            rect.y += height;
            FloorInfo.SetSize(EditorGUI.Vector2IntField(rect, "フロアサイズ", FloorInfo.Size));
            rect.y += height * 2f;

            var originalWidth = rect.width;
            rect.width = rect.width * 0.5f - 20f;

            FloorInfo.SetDeletePathProbability(EditorGUI.Slider(rect, "通路の削除率", FloorInfo.DeletePathProbability, 0f, 1f));
            rect.x += rect.width + 10f;

            FloorInfo.SetMaxRoomCount(EditorGUI.IntSlider(rect, "最大部屋数", FloorInfo.MaxRoomCount, 1, 999));

            rect.x = originalX;
            rect.y += height;

            EditorGUIUtility.labelWidth = 200;

            FloorInfo.SetInitialSpawnEnemyCount(EditorGUI.IntSlider(rect, "侵入時に出現する敵の数", FloorInfo.InitialSpawnEnemyCount, 0, 30));
            rect.x += rect.width + 10f;

            FloorInfo.SetSpawnEnemyIntervalTurn(EditorGUI.IntSlider(rect, "敵が出現する間隔(ターン数)", FloorInfo.SpawnEnemyIntervalTurn, 1, 1000));
            rect.x = originalX;
            rect.y += height;

            FloorInfo.SetInstallTrapMinCount(EditorGUI.IntSlider(rect, "最小罠設置数", FloorInfo.InstallTrapMinNum, 0, FloorInfo.InstallTrapMaxNum));
            rect.x += rect.width + 10f;

            FloorInfo.SetInstallTrapMaxCount(EditorGUI.IntSlider(rect, "最大罠設置数", FloorInfo.InstallTrapMaxNum, FloorInfo.InstallTrapMinNum, 100));
            rect.y += height;
            rect.x = originalX;

            EditorGUIUtility.labelWidth = 150f;

            FloorInfo.SetFloorMaterial(EditorGUI.ObjectField(rect, "床素材", FloorInfo.FloorMaterial, typeof(Material), false) as Material);
            rect.x += rect.width + 10f;
            FloorInfo.SetWallMaterial(EditorGUI.ObjectField(rect, "壁素材", FloorInfo.WallMaterial, typeof(Material), false) as Material);

            rect.x = originalX;
            rect.width = originalWidth;
            rect.y += height;

            EditorGUI.BeginChangeCheck();
            var enemySpawnGroupIndex = DrawPopup(rect, "敵出現パターン", FloorInfo.EnemySpawnGroupId, enemySpawnGroupIdList);
            if (EditorGUI.EndChangeCheck())
                FloorInfo.SetEnemySpawnGroupId(enemySpawnGroupIndex);
            rect.y += height;

            EditorGUI.BeginChangeCheck();
            var trapSettingGroupIndex = DrawPopup(rect, "罠出現パターン", FloorInfo.TrapSettingGroupId, trapSettingGroupIdList);
            if (EditorGUI.EndChangeCheck())
                FloorInfo.SetTrapSettingGroupId(trapSettingGroupIndex);
            rect.y += height;

            FloorInfo.SetSameSettingCount(EditorGUI.IntField(rect, "同じ設定が続く階数", FloorInfo.SameSettingCount));

            rect.y += height;
            foldoutEnemyInfo = EditorGUI.Foldout(rect, foldoutEnemyInfo, "出現する敵情報");
            if (!foldoutEnemyInfo) return;
            rect.y += height;
            rect.width -= 10f;
            rect.x += 10f;
            foreach(var info in DB.Instance.MFloorEnemySpawn.GetByGroupId(FloorInfo.EnemySpawnGroupId))
            {
                var enemy = DB.Instance.MEnemy.GetById(info.EnemyId);
                EditorGUI.LabelField(rect, $"{enemy.Name}: 出現率 {info.Probability}");
                rect.y += height;
            }
        }

        private int DrawPopup(Rect rect, string label, int current, List<int> array)
        {
            try
            {
                var currentIndex = array.IndexOf(current);
                var newIndex = EditorGUI.Popup(rect, label, currentIndex, array.Select(group => $"ID:{group}").ToArray());
                return array[newIndex];
            }
            catch
            {
                return -1;
            }
        }
    }
}
