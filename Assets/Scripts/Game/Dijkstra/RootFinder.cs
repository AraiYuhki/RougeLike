using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dijkstra
{
    /// <summary>
    /// •Ó
    /// </summary>
    public struct Edge
    {
        public int to;
        public int cost;
        public Edge(int to, int cost)
        {
            this.to = to;
            this.cost = cost;
        }
    }

    public enum NodeStatus
    {
        None,
        Open,
        Close
    }

    public class Node
    {
        public int Id => Room.AreaId;
        public NodeStatus Status { get; set; } = NodeStatus.None;
        public Room Room { get; set; }
        public int Score { get; set; } = int.MaxValue;
        public Node Parent { get; set; }
        public Dictionary<int, int> ConnectedCosts { get; set; } = new Dictionary<int, int>();
    }

    public class RootFinder
    {
        private Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        private List<Node> openNodes = new List<Node>();
        private int toId = -1;
        public RootFinder(FloorData data)
        {
            nodes = data.Rooms.ToDictionary(room => room.AreaId, room => new Node() { Room = room });
            foreach(var path in data.Paths)
                nodes[path.FromAreaId].ConnectedCosts[path.ToAreaId] = path.PathPositionList.Count;
        }
        public void Execute(int from, int to)
        {
            toId = to;
            nodes[from].Status = NodeStatus.Open;
            nodes[from].Score = 0;
            openNodes.Add(nodes[from]);
            while(openNodes.Any())
            {
                foreach(var node in openNodes.OrderBy(node => node.Score).ToList())
                    OpenConnected(node);
                openNodes = nodes.Values.Where(node => node.Status == NodeStatus.Open).ToList();
            }
            var goal = nodes[to];
            if (goal.Parent == null)
            {
                Debug.LogError($"Way to goal is not found {from} -> {to}");
                return;
            }

            var current = goal;
            var result = new List<int>();
            while(current != null)
            {
                result.Add(current.Id);
                current = current.Parent;
            }
            result.Reverse();
            Debug.LogError(string.Join("->", result));
        }

        private Node OpenConnected(Node node)
        {
            node.Status = NodeStatus.Close;
            foreach(var next in node.Room.ConnectedRooms)
            {
                var nextNode = nodes[next];
                if (nextNode.Status == NodeStatus.Close) continue;
                nextNode.Status = NodeStatus.Open;
                var nextScore = node.Score + (node.ConnectedCosts.ContainsKey(nextNode.Id) ? node.ConnectedCosts[nextNode.Id] : nextNode.ConnectedCosts[node.Id]);
                if (nextNode.Score > nextScore)
                {
                    Debug.LogError($"Update cost {nextNode.Id} {nextNode.Score} -> {nextScore}");
                    nextNode.Parent = node;
                    nextNode.Score = nextScore;
                }
            }
            return null;
        }
    }
}
