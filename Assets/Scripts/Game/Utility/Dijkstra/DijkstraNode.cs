using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Dijkstra
{
    public enum NodeStatus
    {
        None,
        Open,
        Close
    }

    private const int PathNodeIdOffset = 1000;

    public class Node
    {
        public int Id
        {
            get
            {
                if (IsPathNode)
                    return Path.Id + PathNodeIdOffset;
                return Room.Id;
            }
        }
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

        public Vector3 Position => IsPathNode ? Path.Center : Room.Center;
    }
}
