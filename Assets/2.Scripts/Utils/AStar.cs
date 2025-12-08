using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using PriorityQueue;

public class Node
{
    public Vector3Int position;
    public int gCost;
    public int hCost;
    public Node parent;

    public int fCost => gCost + hCost;

    public override bool Equals(object obj)
    {
        if (obj is Node other)
            return position.Equals(other.position);
        return false;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }
}

public class AStar
{
    public static List<Vector3Int> AStarPathFinding(Vector3Int start, Vector3Int end)
    {
        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        Dictionary<Vector3Int, Node> openSet = new Dictionary<Vector3Int, Node>();

        Node startNode = new Node { position = start, gCost = 0, hCost = Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y), parent = null };

        openList.Enqueue(startNode, startNode.fCost);
        openSet[start] = startNode;

        int[] dirX = {0, 1, 0, -1};
        int[] dirY = {-1, 0, 1, 0};

        Node endNode = null;

        while (openList.Count > 0)
        {
            Node curNode = openList.Dequeue();

            if (closedSet.Contains(curNode)) continue;

            closedSet.Add(curNode);
            openSet.Remove(curNode.position);

            int x = curNode.position.x;
            int y = curNode.position.y;

            if (x == end.x && y == end.y)
            {
                endNode = curNode;
                break;
            }

            int width = Managers.Construct.GridHandler.Width;
            int height = Managers.Construct.GridHandler.Height;

            for (int i = 0; i < 4; i++)
            {
                int nextX = x + dirX[i];
                int nextY = y + dirY[i];
                Vector3Int nextPos = new Vector3Int(nextX, nextY, 0);

                if (nextX < -width / 2 || nextX > width / 2 || nextY < -height / 2 || nextY > height / 2) continue;
                
                Node checkNode = new Node { position = nextPos };
                if (closedSet.Contains(checkNode)) continue;

                TileType tileType = Managers.Construct.GridHandler.GetGridTileType(nextX, nextY);
                if (tileType == TileType.Road)
                {
                    int newGCost = curNode.gCost + 1;

                    if (openSet.TryGetValue(nextPos, out Node existingNode))
                    {
                        if (newGCost < existingNode.gCost)
                        {
                            existingNode.gCost = newGCost;
                            existingNode.parent = curNode;
                            openList.Enqueue(existingNode, existingNode.fCost);
                        }
                    }
                    else
                    {
                        Node nextNode = new Node 
                        { 
                            position = nextPos, 
                            gCost = newGCost, 
                            hCost = Mathf.Abs(nextX - end.x) + Mathf.Abs(nextY - end.y), 
                            parent = curNode 
                        };
                        openList.Enqueue(nextNode, nextNode.fCost);
                        openSet[nextPos] = nextNode;
                    }
                }
            }
        }

        return ReComposePath(endNode);
    }

    private static List<Vector3Int> ReComposePath(Node endNode)
    {
        // 경로 재구성
        List<Vector3Int> path = new List<Vector3Int>();
        if (endNode != null)
        {
            Node currentNode = endNode;
            while (currentNode != null)
            {
                path.Add(currentNode.position);
                currentNode = currentNode.parent;
            }
            path.Reverse();
        }

        return path;
    }
}
