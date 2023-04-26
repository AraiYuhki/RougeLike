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
}
