using System;
using System.Collections.Generic;
// UNITY: Uncomment this line to use it as a Unity component script
// using UnityEngine;

/*
For path finding we're going to use the A* search algorithm:

https://en.wikipedia.org/wiki/A*_search_algorithm

The algorithm is pretty flexible and can work in any number
of dimensions with any coordinate system since you can plug
in different functions for finding neighpors and calculating
the cost of using that neighbor vs others in relation to reaching
the goal / end of the path.  For our usage we're going to keep it
simple and just support a grid where you can only move up, down,
left, and right a cell at a time.  This keeps our cost calculation
simple and fast, and we only need to look at 4 neighbors for each
cell.
*/

/*
To separate the algorithm from the game code that can change,
we just need the game component that controls the grid to
provide a method to find neighbors that are pathable.  Each
pathable neighbor is represented as a vector with the X, Y
location of that cell within the grid.
*/
public interface IGridPathable
{
    List<Vector2Int> GetPathableNeighbors(Vector2Int cell);
}

/*
The path finding utility will take your grid and give you
a path finding method.
*/
// UNITY: Add the : MonoBehaviour back in to use it as a Unity component script
public class GridPathFinder // : MonoBehaviour
{
    public IGridPathable grid;

    // In Unity you can comment out this function because you will use
    // the inspector to provide the grid above
    public GridPathFinder(IGridPathable grid)
    {
        this.grid = grid;
    }

    // This internal class is used to keep track of the neighbors
    // found and their cost to use in the path.
    private class Node: IEquatable<Node>, IComparable<Node>, IComparer<Node>
    {
        public Vector2Int Position;
        public Node? Parent;
        public float G;
        public float H;
        public float F
        {
            get => G + H;
        }

        public Node(Vector2Int position, Node? parent, float g, float h)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
        }

        public override string ToString()
        {
            return $"{Position}[{G}+{H}={F}]";
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public bool Equals(Node? other)
        {
            if (other == null)
            {
                return false;
            }
            Console.WriteLine($"NE: {Position} - {other.Position} - {Position.Equals(other.Position)}");
            return Position.Equals(other.Position);
        }

        public int CompareTo(Node? other) {
            if (other.Equals(this)) {
                return 0;
            }
            if (other == null) {
                return 0;
            }
            return F.CompareTo(other.F);
        }

        public int Compare(Node? x, Node? y)
        {
            throw new NotImplementedException();
        }
    }

    // The heuristic is the cost to use this cell vs another.
    // For this implementation, the cost is just the simple distance
    // between the current cell and the end. 
    private float Heuristic(Vector2Int current, Vector2Int end)
    {
        // Much faster than doing vector distance
        return Math.Abs(current.x - end.x) + Math.Abs(current.y - end.y);
        // return Vector2Int.Distance(current, end);
    }

    // Once a path is found, this will take the linked-list of nodes
    // and turn it back into a list of cell location vectors.
    private List<Vector2Int> ReconstructPath(Node? node)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while (node != null)
        {
            path.Add(node.Position);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    // This is the actual fath-finding method using the A* algorithm.
    // It start with the start cell and then will calculate the cost
    // of moving to each of it's neighbors.
    public List<Vector2Int>? FindPath(Vector2Int start, Vector2Int end) {
        // The open list contains all of the nodes with the cell location
        // and cost sorted by that cost.  Taking a node out of this list (dequeue)
        // weill give you the cheapest node which should be the best node
        // to consider for the next part of the path because it is heading
        // toward the end.
        PriorityQueue<Node, float> openList = new PriorityQueue<Node, float>();
        // The closed set is all of the grid locations we have visited so
        // far, and is used to make sure we don't create two nodes for the
        // same cell putting us in an infinite loop.
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        // Start the path finding with the starting cell
        Node startNode = new Node(start, null, 0, Heuristic(start, end));
        openList.Enqueue(startNode, startNode.F);

        // While we still have cells to consider...
        while (openList.Count > 0) {
            // Grab the cheapest node that should be the best next step
            // in the path
            Node currentNode = openList.Dequeue();
            // If we're at the end, return what we have found as a path
            if (currentNode.Position.Equals(end))
            {
                return ReconstructPath(currentNode);
            }

            // Make sure we know we have checked the current cell
            closedSet.Add(currentNode.Position);
            // For each of the neighbor cells...
            foreach (Vector2Int neighbor in grid.GetPathableNeighbors(currentNode.Position))
            {
                // If we haven't check this neighbor cell, create a node for it,
                // add add it to the open list to be considered as a next step
                // in the path
                if (!closedSet.Contains(neighbor)) {
                    float g = currentNode.G + 1;
                    float h = Heuristic(neighbor, end);
                    Node neighborNode = new Node(neighbor, currentNode, g, h);
                    openList.Enqueue(neighborNode, neighborNode.F);
                }
            }
        }
        // If a path can't be found because the end if blocked, return null
        return null;
    }
}
