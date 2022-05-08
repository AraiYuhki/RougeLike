using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AStar
{
    public class Calculator
    {
        private TileData[,] map;
        private List<Node> openedNode = new List<Node>();
        public Vector2Int StartPoint { get; set; }
        public Vector2Int EndPoint { get; set; }
        private Vector2Int size;

        public Node[,] Nodes { get; private set; }

        public FloorManager FloorManager => ServiceLocator.Instance == null ? null : ServiceLocator.Instance.FloorManager;

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

        public Calculator(TileData[,] map)
        {
            this.map = map;
            size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            Nodes = new Node[size.x, size.y];
            for (var x = 0; x < size.x; x++)
                for (var y = 0; y < size.y; y++)
                    Nodes[x, y] = new Node(x, y);
        }

        public Calculator(TileData[,] map, Vector2Int startPoint, Vector2Int endPoint)
        {
            this.map = map;
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            Nodes = new Node[size.x, size.y];
            for (var x = 0; x < size.x; x++)
                for (var y = 0; y < size.y; y++)
                    Nodes[x, y] = new Node(x, y);
        }

        public List<Vector2Int> Execute()
        {
            Nodes[StartPoint.x, StartPoint.y].State = NodeState.Open;
            openedNode.Add(Nodes[StartPoint.x, StartPoint.y]);
            Node goal = null;
            while (openedNode.Any())
            {
                foreach (var node in openedNode.OrderBy(node => node.Score).ToList())
                {
                    goal = OpenAround(node);
                    if (goal != null)
                        break;
                }
                if (goal != null)
                    break;
                openedNode = Nodes.ToArray().Where(node => node.State == NodeState.Open).ToList();
            }
            if (goal == null)
            {
                Debug.LogError($"Way to goal is not found {StartPoint} -> {EndPoint}");
                return null;
            }
            var current = goal;
            var result = new List<Vector2Int>();
            while(current != null)
            {
                result.Add(current.Position);
                current = current.Parent;
            }
            result.Reverse();
            return result;
        }

        public void Clear()
        {
            for (var x = 0; x < size.x; x++)
                for (var y = 0; y < size.y; y++)
                    Nodes[x, y].Clear();
            openedNode.Clear();
        }

        private Node OpenAround(Node node)
        {
            var position = node.Position;
            node.State = NodeState.Close;
            foreach (var offset in OffsetList)
            {
                var targetPosition = position + offset;
                if (targetPosition.x < 0 || targetPosition.x >= size.x || targetPosition.y < 0 || targetPosition.y >= size.y)
                    continue;
                var targetNode = Nodes[targetPosition.x, targetPosition.y];
                if (map[targetPosition.x, targetPosition.y].IsWall)
                    continue;
                if (targetNode.State != NodeState.None)
                    continue;
                targetNode.State = NodeState.Open;
                targetNode.Parent = node;
                if (offset.x != 0 && offset.y != 0)
                    targetNode.Cost = node.Cost + 1.5f;
                else
                    targetNode.Cost = node.Cost + 1f;
                if (FloorManager != null && FloorManager.GetUnit(targetPosition) != null)
                    targetNode.Cost += 100f;
                targetNode.CalculateEstimatedCost(EndPoint);
                if (targetNode.Position.x == EndPoint.x && targetNode.Position.y == EndPoint.y)
                {
                    return targetNode;
                }
            }
            return null;
        }
    }
}
