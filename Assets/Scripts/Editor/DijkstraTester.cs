using Codice.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;


public class DijkstraTester : EditorWindow
{
    private FloorData floorData;
    private int width = 20;
    private int height = 20;
    private int roomCount = 4;

    private int startId = 0;
    private int endId = 0;

    private Vector3 origin = new Vector2(0, 160f);

    [MenuItem("Tools/ダイクストラtester")]
    public static void Open()
    {
        GetWindow<DijkstraTester>();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("読み込み"))
            LoadFile();
        using (new EditorGUILayout.HorizontalScope())
        {
            width = EditorGUILayout.IntField("幅", width);
            height = EditorGUILayout.IntField("奥行", height);
            roomCount = EditorGUILayout.IntField("部屋数", roomCount);
        }
        if (GUILayout.Button("生成"))
            Generate();
        if (floorData == null) return;
        using(new EditorGUILayout.HorizontalScope())
        {
            var roomIds = floorData.Rooms.Select(room => room.AreaId).ToArray();
            startId = EditorGUILayout.IntPopup("開始地点", startId, roomIds.Select(id => id.ToString()).ToArray(), roomIds);
            endId = EditorGUILayout.IntPopup("終了地点", endId, roomIds.Select(id => id.ToString()).ToArray(), roomIds);
        }
        if (GUILayout.Button("経路探索"))
        {
            var dijkstra = new Dijkstra.RootFinder(floorData);
            dijkstra.Execute(startId, endId);
        }
        //DrawFloorPreview();
        Handles.color = Color.cyan;
        var style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        foreach (var room in floorData.Rooms)
        {
            var position = new Vector3(room.X + room.EndX, room.Y + room.EndY) * 5f + origin;
            Handles.DrawSolidDisc(position, Vector3.forward, 5f);
            Handles.Label(position - Vector3.up * 15f, room.AreaId.ToString(), style);
        }
        Handles.color = Color.white;
        foreach(var path in floorData.Paths)
        {
            var from = floorData.Rooms.First(room => room.AreaId == path.FromAreaId);
            var to = floorData.Rooms.First(room => room.AreaId == path.ToAreaId);
            var fromPosition = new Vector3(from.X + from.EndX, from.Y + from.EndY) * 5f + origin;
            var toPosition = new Vector3(to.X + to.EndX, to.Y + to.EndY) * 5f + origin;
            Handles.DrawLine(fromPosition, toPosition);
            Handles.Label((fromPosition + toPosition) * 0.5f, path.PathPositionList.Count.ToString());
        }
    }

    private void LoadFile()
    {
        var filePath = EditorUtility.OpenFilePanelWithFilters("フロア情報選択", FloorUtil.SavePath, new string[] { "フロア情報", "flr" });
        if (string.IsNullOrEmpty(filePath)) return;
        floorData = FloorUtil.Deserialize(filePath);
        origin.y = floorData.Size.Y;
    }

    private void Generate()
    {
        floorData = DungeonGenerator.GenerateFloor(width, height, roomCount);
        origin.y = floorData.Size.Y;
        Debug.LogError(floorData.Rooms.Count);
    }

    private void DrawFloorPreview()
    {
        if (floorData == null) return;
        var rect = new Rect();
        rect.height = rect.width = 9;
        for (var x = 0; x < floorData.Size.X; x++)
        {
            for (var y = 0; y < floorData.Size.Y; y++)
            {
                rect.position = new Vector2(x * 10, y * 10) + new Vector2(origin.x, origin.y);
                EditorGUI.DrawRect(rect, NodeColor(floorData.Map[x, y]));
            }
        }
    }

    private Color NodeColor(TileData tile)
    {
        if (floorData.StairPosition == tile.Position)
            return Color.magenta;
        else if (floorData.SpawnPoint == tile.Position)
            return Color.green;
        return tile.Type == TileType.Wall ? Color.black : Color.white;
    }
}
