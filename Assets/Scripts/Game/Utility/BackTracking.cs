﻿using System.Collections.Generic;
using System.Linq;
using static Dijkstra;

public static class BackTracking
{
    public static List<Room> FindIsolatedRoom(List<Room> rooms, List<Path> paths)
    {
        var nodes = rooms.ToDictionary(room => room.Id, room => new Node() { Room = room });
        foreach (var path in paths)
            nodes[path.FromRoomId].ConnectedCosts[path.ToRoomId] = path.PathPositionList.Count;

        var start = rooms.Random().Id;
        nodes[start].Status = NodeStatus.Open;
        var openNodes = new List<Node>() { nodes[start] };
        var searchedRooms = new List<Room>();
        while (openNodes.Any())
        {
            foreach (var node in openNodes.ToList())
            {
                node.Status = NodeStatus.Close;
                searchedRooms.Add(node.Room);
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
        if (nodes.Any(node => node.Value.Status != NodeStatus.Close)) 
            return searchedRooms;
        return null;
    }
}
