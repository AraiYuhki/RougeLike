using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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

    private FloorData floorData;
    private Vector2Int size;

    private IUnitContainer unitContainer;

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

    public AStar()
    {
    }

    public AStar(FloorData floorData, IUnitContainer container)
    {
        this.floorData = floorData;
        this.unitContainer = container;
        size = floorData.Size;
    }

    public void Setup(FloorData floorData, IUnitContainer unitContainer)
    {
        this.floorData = floorData;
        this.unitContainer = unitContainer;
        size = floorData.Size;
    }

    public List<Vector2Int> FindRoot(Vector2Int startPoint, Vector2Int endPoint)
    {
        var nodes = new Node[size.x, size.y];
        for (var x = 0; x < size.x; x++)
        {
            for (var y = 0; y < size.y; y++)
            {
                var node = new Node(x, y);
                if (unitContainer != null && unitContainer.ExistsUnit(node.Position))
                    node.Cost += 100;
                nodes[x, y] = node;
            }
        }
        var result = new List<Vector2Int>();
        var openedNode = new List<Node>();
        FindRoot(endPoint, nodes[startPoint.x, startPoint.y], nodes, ref result, ref openedNode);
        return result;
    }

    private bool FindRoot(Vector2Int endPoint, Node current, Node[,] nodes, ref List<Vector2Int> result, ref List<Node> openedNode)
    {
        current.State = NodeState.Close;
        openedNode.Remove(current);
        var goal = OpenAround(endPoint, nodes, current, ref openedNode);
        if (goal != null)
        {
            result = CreateRoot(goal);
            return true;
        }
        while(openedNode.Count > 0)
        {
            var next = openedNode.OrderBy(node => node.Score).First();
            if (FindRoot(endPoint, next, nodes, ref result, ref openedNode))
                return true;
        }
        return false;
    }

    private List<Vector2Int> CreateRoot(Node current)
    {
        var tmp = current;
        var result = new List<Vector2Int>() { current.Position };
        while (tmp != null)
        {
            result.Add(tmp.Position);
            tmp = tmp.Parent;
        }
        result.Reverse();
        return result;
    }

    private Node OpenAround(Vector2Int endPoint, Node[,] nodes, Node node, ref List<Node> openedNode)
    {
        var position = node.Position;
        node.State = NodeState.Close;
        foreach (var offset in OffsetList)
        {
            var targetPosition = position + offset;
            if (targetPosition.X < 0 || targetPosition.X >= size.x || targetPosition.Y < 0 || targetPosition.Y >= size.y)
                continue;
            var targetNode = nodes[targetPosition.X, targetPosition.Y];
            if (floorData.Map[targetPosition.X, targetPosition.Y].IsWall)
                continue;
            if (targetNode.State != NodeState.None)
                continue;
            openedNode.Add(targetNode);
            targetNode.State = NodeState.Open;
            targetNode.Parent = node;
            if (offset.x != 0 && offset.y != 0)
                targetNode.Cost = node.Cost + 1.5f;
            else
                targetNode.Cost = node.Cost + 1f;
            if (unitContainer != null && unitContainer.ExistsUnit(targetPosition))
                targetNode.Cost += 100f;
            targetNode.CalculateEstimatedCost(endPoint);
            if (targetNode.Position.X == endPoint.x && targetNode.Position.Y == endPoint.y)
            {
                return targetNode;
            }
        }
        return null;
    }
}
