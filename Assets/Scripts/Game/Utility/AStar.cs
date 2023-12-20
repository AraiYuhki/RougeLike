using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar
{
    public enum NodeState
    {
        None,
        Open,
        Close
    }

    public class Node
    {
        public Point Position { get; set; } = new Point(0, 0);
        public float Cost { get; set; } = 0;
        public float Score { get; set; } = 0;
        public int EstimatedCost { get; set; } = 0;
        public NodeState State { get; set; } = NodeState.None;
        public Node Parent { get; set; } = null;
        public Node() { }
        public Node(int x, int y) => Position = new Point(x, y);

        public void CalculateEstimatedCost(Point endPoint)
        {
            var x = Mathf.Abs(Position.X - endPoint.X);
            var y = Mathf.Abs(Position.Y - endPoint.Y);
            EstimatedCost = Mathf.Max(x, y);
            Score = Cost + EstimatedCost;
        }

        public void Clear()
        {
            Cost = Score = EstimatedCost = 0;
            State = NodeState.None;
            Parent = null;
        }
    }

    private TileData[,] map;
    public Vector2Int StartPoint { get; set; }
    public Vector2Int EndPoint { get; set; }
    private Vector2Int size;

    private Node[,] nodes;

    private FloorManager floorManager;

    private static readonly Vector2Int[] OffsetList = new Vector2Int[]{
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.up + Vector2Int.left,
            Vector2Int.down + Vector2Int.left
            };

    public AStar(TileData[,] map, FloorManager floorManager)
    {
        this.map = map;
        this.floorManager = floorManager;
        size = new Vector2Int(map.GetLength(0), map.GetLength(1));
        nodes = new Node[size.x, size.y];
        for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
                nodes[x, y] = new Node(x, y);
    }

    public AStar(TileData[,] map, Vector2Int startPoint, Vector2Int endPoint)
    {
        this.map = map;
        StartPoint = startPoint;
        EndPoint = endPoint;
        size = new Vector2Int(map.GetLength(0), map.GetLength(1));
        nodes = new Node[size.x, size.y];
        for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
                nodes[x, y] = new Node(x, y);
    }

    public List<Vector2Int> Execute(int limit = -1)
    {
        try
        {
            Clear();
            nodes[StartPoint.x, StartPoint.y].State = NodeState.Open;
            var openedNode = new List<Node>{ nodes[StartPoint.x, StartPoint.y] };
            Node goal = null;
            var count = 0;
            while (openedNode.Count > 0)
            {
                count++;
                foreach (var node in openedNode.OrderBy(node => node.Score).ToList())
                {
                    goal = OpenAround(node);
                    if (goal != null)
                        break;
                }
                if (goal != null)
                    break;
                openedNode = nodes.ToArray().Where(node => node.State == NodeState.Open).ToList();
                if (limit > 0 && count >= limit)
                {
                    goal = openedNode.OrderBy(node => node.Score).First();
                    break;
                }
            }

            if (goal == null)
            {
                Debug.LogWarning($"Way to goal is not found {StartPoint} -> {EndPoint}");
                return null;
            }
            var current = goal;
            var result = new List<Vector2Int>();
            while (current != null)
            {
                result.Add(current.Position);
                current = current.Parent;
            }
            result.Reverse();
            return result;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw e;
        }
    }

    public void Clear()
    {
        for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
                nodes[x, y].Clear();
    }

    private Node OpenAround(Node node)
    {
        var position = node.Position;
        node.State = NodeState.Close;
        foreach (var offset in OffsetList)
        {
            var targetPosition = position + offset;
            if (targetPosition.X < 0 || targetPosition.X >= size.x || targetPosition.Y < 0 || targetPosition.Y >= size.y)
                continue;
            var targetNode = nodes[targetPosition.X, targetPosition.Y];
            if (map[targetPosition.X, targetPosition.Y].IsWall)
                continue;
            if (targetNode.State != NodeState.None)
                continue;
            targetNode.State = NodeState.Open;
            targetNode.Parent = node;
            if (offset.x != 0 && offset.y != 0)
                targetNode.Cost = node.Cost + 1.5f;
            else
                targetNode.Cost = node.Cost + 1f;
            if (floorManager != null && floorManager.GetUnit(targetPosition) != null)
                targetNode.Cost += 100f;
            targetNode.CalculateEstimatedCost(EndPoint);
            if (targetNode.Position.X == EndPoint.x && targetNode.Position.Y == EndPoint.y)
            {
                return targetNode;
            }
        }
        return null;
    }
}
