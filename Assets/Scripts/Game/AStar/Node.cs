using UnityEngine;

namespace AStar
{
    public enum NodeState
    {
        None,
        Open,
        Close
    }

    public class Node
    {
        public Vector2Int Position { get; set; } = Vector2Int.zero;
        public float Cost { get; set; } = 0;
        public float Score { get; set; } = 0;
        public int EstimatedCost { get; set; } = 0;
        public NodeState State { get; set; } = NodeState.None;
        public Node Parent { get; set; } = null;
        public Node() { }
        public Node(int x, int y) => Position = new Vector2Int(x, y);

        public void CalculateEstimatedCost(Vector2Int endPoint)
        {
            var x = Mathf.Abs(Position.x - endPoint.x);
            var y = Mathf.Abs(Position.y - endPoint.y);
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
}
