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
    private float deletePercent = 30f;

    private int startId = 0;
    private int endId = 0;

    private bool drawMap = false;
    private bool visibleDeleted = false;

    private Vector3 origin = new Vector2(0, 130f);

    [MenuItem("Tools/�_�C�N�X�g��tester")]
    public static void Open()
    {
        GetWindow<DijkstraTester>();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("�ǂݍ���"))
            LoadFile();
        using (new EditorGUILayout.HorizontalScope())
        {
            width = EditorGUILayout.IntField("��", width);
            height = EditorGUILayout.IntField("���s", height);
            roomCount = EditorGUILayout.IntField("������", roomCount);
            deletePercent = EditorGUILayout.Slider("�ʘH�폜�m��", deletePercent, 0f, 1f);
        }
        if (GUILayout.Button("����"))
            Generate();
        if (floorData == null) return;
        using(new EditorGUILayout.HorizontalScope())
        {
            var roomIds = floorData.Rooms.Select(room => room.AreaId).ToArray();
            startId = EditorGUILayout.IntPopup("�J�n�n�_", startId, roomIds.Select(id => id.ToString()).ToArray(), roomIds);
            endId = EditorGUILayout.IntPopup("�I���n�_", endId, roomIds.Select(id => id.ToString()).ToArray(), roomIds);
        }
        if (GUILayout.Button("�o�H�T��"))
        {
            var dijkstra = new Dijkstra.RootFinder(floorData);
            dijkstra.Execute(startId, endId);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            drawMap = EditorGUILayout.Toggle("�}�b�v�`��", drawMap);
            visibleDeleted = EditorGUILayout.Toggle("�폜�����o�H��\��", visibleDeleted);
        }
        if (drawMap)
            DrawFloorPreview();
        else
            DrawGraph();
    }

    private void LoadFile()
    {
        var filePath = EditorUtility.OpenFilePanelWithFilters("�t���A���I��", FloorUtil.SavePath, new string[] { "�t���A���", "flr" });
        if (string.IsNullOrEmpty(filePath)) return;
        floorData = FloorUtil.Deserialize(filePath);
    }

    private void Generate()
    {
        floorData = DungeonGenerator.GenerateFloor(width, height, roomCount, deletePercent);
        Debug.LogError(floorData.Rooms.Count);
    }
    private void DrawGraph()
    {
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
        foreach (var path in floorData.Paths)
        {
            var from = floorData.Rooms.First(room => room.AreaId == path.FromAreaId);
            var to = floorData.Rooms.First(room => room.AreaId == path.ToAreaId);
            var fromPosition = new Vector3(from.X + from.EndX, from.Y + from.EndY) * 5f + origin;
            var toPosition = new Vector3(to.X + to.EndX, to.Y + to.EndY) * 5f + origin;
            Handles.DrawLine(fromPosition, toPosition);
            Handles.Label((fromPosition + toPosition) * 0.5f, path.PathPositionList.Count.ToString());
        }
    }

    private void DrawFloorPreview()
    {
        if (floorData == null) return;
        var rect = new Rect();
        rect.height = rect.width = 9;
        var origin = new Vector2(0, 200f);
        for (var x = 0; x < floorData.Size.X; x++)
        {
            for (var y = 0; y < floorData.Size.Y; y++)
            {
                rect.position = new Vector2(x * 10, y * 10) + origin;
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
        switch (tile.Type)
        {
            case TileType.Wall:
                return Color.black;
            case TileType.Hole:
                return Color.blue;
            case TileType.Deleted:
                return visibleDeleted ? Color.red : Color.black;
            default:
                return Color.white;
        }
    }
}
