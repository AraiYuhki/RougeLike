using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class Dijkstra
{
    public enum NodeStatus
    {
        None,
        Open,
        Close
    }

    public class Node
    {
        public int Id { get; set; }
        public NodeStatus Status { get; set; } = NodeStatus.None;
        public Room Room { get; set; } = null;
        public Path Path { get; set; } = null;
        public int Score { get; set; } = int.MaxValue;
        public Node Parent { get; set; }
        public Dictionary<int, int> ConnectedCosts { get; set; } = new Dictionary<int, int>();
        public bool IsTmporary { get; set; } = false;
        public bool IsPathNode => Path != null;

        public void Clear()
        {
            Score = int.MaxValue;
            Parent = null;
            Status = NodeStatus.None;
        }

        public Vector3 Position
        {
            get
            {
                if (Room != null)
                    return Room.Center;
                if (Path != null)
                    return Path.Center;
                return Vector3.zero;
            }
        }
    }

    private Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    private List<Node> openNodes = new List<Node>();
    private FloorData floorData;
    private bool hasTmpNode = false;

    public Dictionary<int, Node> Nodes => nodes;
    public Dijkstra(FloorData data)
    {
        floorData = data;
        Initialize();
    }

    public List<int> GetRoot(int from, int to)
    {
        nodes[from].Status = NodeStatus.Open;
        nodes[from].Score = 0;
        openNodes.Add(nodes[from]);
        while (openNodes.Any())
        {
            foreach (var node in openNodes.OrderBy(node => node.Score).ToList())
                OpenConnected(node);
            openNodes = nodes.Values.Where(node => node.Status == NodeStatus.Open).ToList();
        }
        var goal = nodes[to];
        if (goal.Parent == null)
        {
            Debug.LogWarning($"Way to goal is not found {from} -> {to}");
            return null;
        }

        var current = goal;
        var result = new List<int>();
        while (current != null)
        {
            result.Add(current.Id);
            current = current.Parent;
        }
        result.Reverse();
        Debug.Log(string.Join("->", result));
        return result;
    }

    private int AddTmpNode(Path path)
    {
        var halfCost = path.PathPositionList.Count / 2;
        var tmpId = -path.Id;
        var tmpNode = new Node() { Id = tmpId, IsTmporary = true, Path = path };
        tmpNode.ConnectedCosts.Add(path.FromRoomId, halfCost);
        tmpNode.ConnectedCosts.Add(path.ToRoomId, halfCost);
        
        nodes[path.FromRoomId].ConnectedCosts.Remove(path.ToRoomId);
        nodes[path.ToRoomId].ConnectedCosts.Remove(path.FromRoomId);

        nodes[path.FromRoomId].ConnectedCosts[tmpId] = halfCost;
        nodes[path.ToRoomId].ConnectedCosts[tmpId] = halfCost;
        nodes.Add(tmpId, tmpNode);
        hasTmpNode = true;
        return tmpId;
    }

    public List<int> GetRoot(TileData start, TileData end)
    {
        if (hasTmpNode) Initialize();
        var startId = start.Id;
        var endId = end.Id;
        if (!start.IsRoom)
        {
            var path = floorData.Paths.FirstOrDefault(path => path.Id == start.Id);
            // 現在位置が通路の場合は一時的なノードを作成する
            startId = AddTmpNode(path);
        }
        if (!end.IsRoom)
        {
            var path = floorData.Paths.FirstOrDefault(path => path.Id == start.Id);
            endId = AddTmpNode(path);
        }
        return GetRoot(startId, endId);
    }

    public List<Vector2Int> GetCheckpoints(Vector2Int start, Vector2Int end)
    {
        var startTile = floorData.Map[start.x, start.y];
        var endTile = floorData.Map[end.x, end.y];
        var nodeList = GetRoot(startTile, endTile);
        var checkPoints = new List<Vector2Int>();
        
        if (nodeList == null) return checkPoints;

        for (var index = 0;index < nodeList.Count; index++)
        {
            var currentNode = nodes[nodeList[index]];
            // 原則通路ノードは最初のノードにしかないはずなので、それ以外のパターンは許容しない
            if (index == 0)
            {
                // 最初のノードが通路の場合は何もしない
                if (!currentNode.IsPathNode)
                {
                    var nextNode = nodes[nodeList[index + 1]];
                    checkPoints.Add(currentNode.Room.ConnectedPoint[nextNode.Id]);
                }
                continue;
            }
            var prevNode = nodes[nodeList[index - 1]];
            var prevRoomId = 0;
            // ひとつ前のノードが通路の場合は通路の接続先から部屋のIDを類推し、チェックポイントの座標を探す
            if (prevNode.IsPathNode)
                prevRoomId = prevNode.Path.ToRoomId == currentNode.Id ? prevNode.Path.FromRoomId : prevNode.Path.ToRoomId;
            else
                prevRoomId = prevNode.Id;
            checkPoints.Add(currentNode.Room.ConnectedPoint[prevRoomId]);

            // 最後のノードなら目的地を追加
            if (index == nodeList.Count - 1)
                checkPoints.Add(endTile.Position);
            else
            {
                var nextNode = nodes[nodeList[index + 1]];
                var nextRoomId = 0;
                if (nextNode.IsPathNode)
                    nextRoomId = nextNode.Path.ToRoomId == currentNode.Id ? nextNode.Path.FromRoomId : nextNode.Path.ToRoomId;
                else
                    nextRoomId = nextNode.Id;
                checkPoints.Add(currentNode.Room.ConnectedPoint[nextRoomId]);
            }
        }
        return checkPoints;
    }

    public List<Vector2Int> GetRoot(Vector2Int startPoint, Vector2Int endPoint, List<Vector2Int> checkPoints)
    {
        var points = new List<Vector2Int>() { startPoint };
        points.AddRange(checkPoints);
        points.Add(endPoint);
        var root = new List<Vector2Int>();
        var aStar = new AStar(floorData.Map, GameObject.FindObjectOfType<FloorManager>());
        for (var index = 0; index < points.Count - 2; index++)
        {
            aStar.StartPoint = points[index];
            aStar.EndPoint = points[index + 1];
            var positions = aStar.Execute(15);
            if (positions != null) root.AddRange(positions);
        }
        return root;
    }

    private Node OpenConnected(Node node)
    {
        node.Status = NodeStatus.Close;
        if (nodes.Count <= 0)
        {
            Debug.LogWarning("Nodes is empty");
            return null;
        }
        foreach (var next in node.ConnectedCosts.Keys)
        {
            if (!nodes.ContainsKey(next))
            {
                Debug.LogWarning($"{next} is not in nodes");
                continue;
            }
            var nextNode = nodes[next];
            if (nextNode.Status == NodeStatus.Close) continue;
            nextNode.Status = NodeStatus.Open;
            var nextScore = node.Score + node.ConnectedCosts[nextNode.Id];
            if (nextNode.Score > nextScore)
            {
                Debug.Log($"Update cost {nextNode.Id} {nextNode.Score} -> {nextScore}");
                nextNode.Parent = node;
                nextNode.Score = nextScore;
            }
        }
        return null;
    }

    public static List<Room> FindClosedPath(List<Room> rooms, List<Path> paths)
    {
        var nodes = rooms.ToDictionary(room => room.Id, room => new Node() { Id = room.Id, Room = room });
        foreach (var path in paths)
            nodes[path.FromRoomId].ConnectedCosts[path.ToRoomId] = path.PathPositionList.Count;

        var start = rooms.Random().Id;
        nodes[start].Status = NodeStatus.Open;
        var openNodes = new List<Node>() { nodes[start] };
        var checkedNodes = new List<Room>();
        while (openNodes.Any())
        {
            foreach (var node in openNodes.ToList())
            {
                node.Status = NodeStatus.Close;
                checkedNodes.Add(node.Room);
                openNodes.Remove(node);
                foreach (var next in node.Room.ConnectedRooms)
                {
                    var nextNode = nodes[next];
                    if (nextNode.Status == NodeStatus.Close) continue;
                    nextNode.Status = NodeStatus.Open;
                    openNodes.Add(nextNode);
                }
            }
        }
        if (nodes.Where(node => node.Value.Status != NodeStatus.Close).Any()) return checkedNodes;
        return null;
    }

    private void Initialize()
    {
        hasTmpNode = false;
        nodes = floorData.Rooms.ToDictionary(room => room.Id, room => new Node() { Id = room.Id, Room = room });
        foreach (var path in floorData.Paths)
        {
            nodes[path.FromRoomId].ConnectedCosts[path.ToRoomId] = path.PathPositionList.Count;
            nodes[path.ToRoomId].ConnectedCosts[path.FromRoomId] = path.PathPositionList.Count;
        }
    }
}

