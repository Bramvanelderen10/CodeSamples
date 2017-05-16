using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading;

/// <summary>
/// Grid based astar algorithm
/// Verical/Horizontal movement cost 10, diagonal is 14, diagonal to corners is 17
/// </summary>
public class Astar : MonoBehaviour
{
    public delegate void ActionPath(List<ANode> path);

    [SerializeField] private LayerMask _layer;
    [SerializeField] private Vector3 _nodeHalfExtends; //Defines how big each node is
    private ANode[,,] _nodesArray;
    private bool _isSearching = false;
    private int _width; //Width of the grid (Columns)
    private int _length; //Length of the grid (Rows)
    private int _height;
    private ActionPath _actionPath; //Is used when the path is generated


    /// <summary>
    /// Generates a node grid
    /// </summary>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public void GenerateGrid(int width, int length, int height, Vector3 center)
    {
        if (_isSearching)
            return;

        _width = width;
        _length = length;
        _height = height;
        _nodesArray = new ANode[length, width, height];

        var positionalOffset = center +
                               new Vector3(-_nodeHalfExtends.x*width, -_nodeHalfExtends.y*height,
                                   -_nodeHalfExtends.z*length);

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    var node = new ANode();
                    node.Position = positionalOffset + new Vector3((j * (_nodeHalfExtends.x * 2)) + (_nodeHalfExtends.x), (k * (_nodeHalfExtends.y * 2) + (_nodeHalfExtends.y)), (i * (_nodeHalfExtends.z * 2)) + (_nodeHalfExtends.z));
                    node.gIndex = new int[3];
                    node.gIndex[0] = i;
                    node.gIndex[1] = j;
                    node.gIndex[2] = k;
                    _nodesArray[i, j, k] = node;
                }
            }
        }
    }

    /// <summary>
    /// Initiates the algorithm
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="targetPosition"></param>
    /// <param name="action"></param>
    public void GetPath(Vector3 startPosition, Vector3 targetPosition, ActionPath action)
    {
        if (_isSearching)
            return;

        _actionPath = action;
        StartCoroutine(FindPath(FindNearestNode(startPosition), FindNearestNode(targetPosition)));
    }



    /// <summary>
    /// Finds the position of the next node from the nearest node found
    /// Can be used externally to find where to go based on a path array and current position
    /// </summary>
    /// <param name="path"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    public Vector3 FindWhereToGo(List<ANode> path, Vector3 current)
    {
        var currentNode = FindNearestNode(path, current);

        int index = path.FindIndex(x => x == currentNode);
        if (index < path.Count - 1)
            index++;

        return path[index].Position;
    }

    /// <summary>
    /// The algorithm itself, in an ienumerator so we can split the logic over multiple frames for performance
    /// </summary>
    /// <param name="maxDuration"></param>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    IEnumerator FindPath(ANode start, ANode target)
    {
        _isSearching = true;
        SetupNodes(target);
        yield return null;

        var traverseThread = new Thread(() => TraverseNodes(start, target));
        traverseThread.Start();
        while (traverseThread.IsAlive)
            yield return null;

        _actionPath(RetracePath(start, target)); //Callback


        _isSearching = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    void SetupNodes(ANode target)
    {
        //Reset all nodes to default values and set availability and H value
        foreach (var node in _nodesArray)
        {
            //Calculate h value
            node.H = Mathf.Abs(node.gIndex[1] - target.gIndex[1]) + Mathf.Abs(node.gIndex[0] - target.gIndex[0]) + Mathf.Abs(node.gIndex[2] - target.gIndex[2]);
            node.G = 0;
            node.Parent = null;
            node.Available = true;
            if (Physics.CheckBox(node.Position, _nodeHalfExtends, Quaternion.identity, _layer))
                node.Occupied = true;
        }
    }

    /// <summary>
    /// Traverses all nodes based on a given start and target value
    /// Can be threaded
    /// </summary>
    /// <param name="openTree"></param>
    /// <param name="closedNodes"></param>
    /// <param name="target"></param>
    void TraverseNodes(ANode start, ANode target)
    { 
        BTree<ANode> openTree = new BTree<ANode>();
        List<ANode> closedNodes = new List<ANode>();
        openTree.Add(start);
        start.Available = false;
        while (openTree.Count != 0)
        {
            var current = openTree.Min();
            if (current == target)
                break; //Reached the target so its done

            for (int i = -1; i < 2; i++) // Loop from row -1 to 1 based on the current row
            {
                for (int j = -1; j < 2; j++) // Loop from column -1 to 1 based on the current column
                {
                    for (int k = -1; k < 2; k++)
                    {
                        //Filter edge cases
                        if (i == -1 && current.gIndex[0] == 0)
                            continue;
                        if (i == 1 && current.gIndex[0] == _length - 1)
                            continue;
                        if (j == -1 && current.gIndex[1] == 0)
                            continue;
                        if (j == 1 && current.gIndex[1] == _width - 1)
                            continue;
                        if (k == -1 && current.gIndex[2] == 0)
                            continue;
                        if (k == 1 && current.gIndex[2] == _height - 1)
                            continue;

                        var node = _nodesArray[current.gIndex[0] + i, current.gIndex[1] + j, current.gIndex[2] + k];
                        if (!node.Available || node.Occupied)
                            continue;

                        //Check if diagonal, if so more G cost
                        if (i == -1 && j == -1 && k == -1 ||
                            i == -1 && j == -1 && k == 1 ||
                            i == -1 && j == 1 && k == -1 ||
                            i == -1 && j == 1 && k == 1 ||

                            i == 1 && j == -1 && k == -1 ||
                            i == 1 && j == -1 && k == 1 ||
                            i == 1 && j == 1 && k == -1 ||
                            i == 1 && j == 1 && k == 1)
                        {
                            node.G = current.G + 17;
                        }
                        else if (
                            i == -1 && j == -1 && k == 0 ||
                            i == -1 && j == 1 && k == 0 ||
                            i == 1 && j == -1 && k == 0 ||
                            i == 1 && j == 1 && k == 0 ||

                            i == -1 && k == -1 && j == 0 ||
                            i == -1 && k == 1 && j == 0 ||
                            i == 1 && k == -1 && j == 0 ||
                            i == 1 && k == 1 && j == 0 ||

                            k == -1 && j == -1 && i == 0 ||
                            k == -1 && j == 1 && i == 0 ||
                            k == 1 && j == -1 && i == 0 ||
                            k == 1 && j == 1 && i == 0
                            )
                        {
                            node.G = current.G + 14;
                        }
                        else
                        {
                            node.G = current.G + 10;
                        }

                        node.Parent = current;
                        node.Available = false;

                        openTree.Add(node);
                    }
                }
            }
            openTree.Remove(current);
            closedNodes.Add(current);
        }
    }

    /// <summary>
    /// Retraces the found path based on parents
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    List<ANode> RetracePath(ANode start, ANode end)
    {
        var path = new List<ANode>();
        path.Add(end);
        while (path[path.Count - 1] != start)
        {
            if (path[path.Count - 1].Parent == null)
                break;

            path.Add(path[path.Count - 1].Parent);
        }
        path.Reverse();

        return path;
    }

    /// <summary>
    /// Finds the nearest node of all existing nodes based on position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    ANode FindNearestNode(Vector3 position)
    {
        var result = _nodesArray[0, 0, 0];
        foreach (var node in _nodesArray)
        {
            if (Vector3.Distance(node.Position, position) < Vector3.Distance(result.Position, position))
                result = node;
        }

        return result;
    }

    /// <summary>
    /// Finds the nearest node in a node list based on position
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    ANode FindNearestNode(List<ANode> nodes, Vector3 position)
    {
        var result = nodes[0];
        foreach (var node in nodes)
        {
            if (Vector3.Distance(node.Position, position) < Vector3.Distance(result.Position, position))
                result = node;
        }

        return result;
    }
}

/// <summary>
/// A* node
/// </summary>
public class ANode : IComparable<ANode>
{
    public int[] gIndex; //is an array of the row ([0]) column ([1]) index
    public Vector3 Position; // Is used to find the nearest node and to find where to go
    public ANode Parent; // The parenting structure used to retrace the found path
    public float H; //The estimate from this node to target node
    public float G; //The value required to travel to this node from start
    public bool Available = true; //Should this node be included in the search
    public bool Occupied = false;

    public float F
    {
        get { return H + G; }
    }

    public int CompareTo(ANode other)
    {
        if (other == null)
            throw new ArgumentException();

        if (this == other)
            return 0;

        var result = this.F.CompareTo(other.F);
        if (result == 0)
            result = 1;

        return result;
    }
}
