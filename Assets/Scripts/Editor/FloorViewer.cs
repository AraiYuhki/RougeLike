using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FloorViewer : EditorWindow
{
    private static FloorData floorData = null;
    private static Vector2 Origin = new Vector2(0, 120f);
    private static AStar.Calculator calculator;
    private static List<Vector2Int> root = new List<Vector2Int>();

    [MenuItem("Tools/フロアビュアー")]
    public static void Open()
    {
        GetWindow<FloorViewer>();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("読み込み"))
            ReadFile();
        if (GUILayout.Button("経路探索A*"))
        {
            calculator = new AStar.Calculator(floorData.Map, floorData.SpawnPoint, floorData.StairPosition);
            root = calculator.Execute();
        }
        if (GUILayout.Button("経路探索ダイクストラ"))
        {
            var dijkstra = new Dijkstra.RootFinder(floorData);
            dijkstra.Execute(floorData.Rooms.First().AreaId, floorData.Rooms.Last().AreaId);
        }
        DrawFloorPreview();
    }

    private void ReadFile()
    {
        var filePath = EditorUtility.OpenFilePanelWithFilters("フロア情報選択", FloorUtil.SavePath, new string[] { "フロア情報", "flr" });
        floorData = FloorUtil.Deserialize(filePath);
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
                rect.position = new Vector2(x * 10, y * 10) + Origin;
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
        else if (root.Any(r => r.x == tile.Position.X && r.y == tile.Position.Y))
            return Color.cyan;
        return tile.Type == TileType.Wall ? Color.black : Color.white;
    }

    private Color NodeColor(AStar.Node node)
    {
        switch(node.State)
        {
            case AStar.NodeState.None:
                return Color.white;
            case AStar.NodeState.Open:
                return Color.cyan;
            case AStar.NodeState.Close:
                return Color.gray;
            default:
                return Color.black;
        }
    }
}
