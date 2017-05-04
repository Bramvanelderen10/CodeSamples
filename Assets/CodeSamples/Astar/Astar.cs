using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Grid based astar algorithm
/// Verical/Horizontal movement cost 10, diagonal is 14
/// </summary>
public class Astar : MonoBehaviour
{
    public delegate void ActionPath(List<ANode> path);

    [SerializeField] private LayerMask _layer;
    [SerializeField] private Vector3 _nodeHalfExtends; //Defines how big each node is
    [SerializeField] private int _searchDuration = 300;
    private ANode[,] _nodesArray;
    private bool _isSearching = false;
    private int _width; //Width of the grid (Columns)
    private int _length; //Length of the grid (Rows)
    private ActionPath _actionPath; //Is used when the path is generated


    /// <summary>
    /// Generates a node grid
    /// </summary>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public void GenerateGrid(int width, int length)
    {
        if (_isSearching)
            return;

        _width = width;
        _length = length;
        _nodesArray = new ANode[length, width];

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var node = new ANode();
                node.Position = new Vector3((j * (_nodeHalfExtends.x * 2)) + (_nodeHalfExtends.x), transform.position.y, (i * (_nodeHalfExtends.z * 2)) + (_nodeHalfExtends.z));
                node.gIndex = new int[2];
                node.gIndex[0] = i;
                node.gIndex[1] = j;
                _nodesArray[i, j] = node;
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
        List<ANode> _openNodes = new List<ANode>();
        List<ANode> _closedNodes = new List<ANode>();


        int waitFrameInterval = Mathf.FloorToInt(_nodesArray.Length / ((_searchDuration / 10) / (float)(10000 / _nodesArray.Length))); //Estimated interval value to improve performance over long distance paths
        int counter = 0;
        //Reset all nodes to default values and set availability and H value
        foreach (var node in _nodesArray)
        {
            //Split loop over multiple frames
            counter++;
            if (counter >= waitFrameInterval)
            {
                counter = 0;
                yield return null;
            }
            if (Physics.CheckBox(node.Position, _nodeHalfExtends, Quaternion.identity, _layer))
                continue;

            //Calculate h value
            node.H = Mathf.Abs(node.gIndex[1] - target.gIndex[1]) + Mathf.Abs(node.gIndex[0] - target.gIndex[0]);
            node.G = 0;
            node.Parent = null;
            node.Available = true;
        }
        yield return null; //Wait a frame
        _openNodes.Add(start);
        start.Available = false;
        waitFrameInterval = Mathf.FloorToInt(_nodesArray.Length/ (_searchDuration / (float)(10000 / _nodesArray.Length))); //Estimated interval value to improve performance over long distance paths
        counter = 0;
        while (_openNodes.Count != 0)
        {
            //Split loop over multiple frames
            counter++;
            if (counter >= waitFrameInterval)
            {
                counter = 0;
                yield return null;
            }

            var current = _openNodes.Aggregate((x1, x2) => x1.F < x2.F ? x1 : x2); //Find the node with the lowest F value
            if (current == target)
                break; //Reached the target so its done

            for (int i = -1; i < 2; i++) // Loop from row -1 to 1 based on the current row
            {
                for (int j = -1; j < 2; j++) // Loop from column -1 to 1 based on the current column
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

                    var node = _nodesArray[current.gIndex[0] + i,current.gIndex[1] + j];
                    if (!node.Available)
                        continue;

                    //Check if diagonal, if so more G cost
                    if (i == -1 && j == -1 ||
                        i == -1 && j == 1 ||
                        i == 1 && j == -1 ||
                        i == 1 && j == 1)
                        node.G = current.G + 14;
                    else
                        node.G = current.G + 10;

                    node.Parent = current;
                    node.Available = false;
                    _openNodes.Add(node);
                }
            }
            _openNodes.Remove(current);
            _closedNodes.Add(current);
        }
        _isSearching = false;
        _actionPath(RetracePath(start, target)); //Callback
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
        var result = _nodesArray[0, 0];
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

    /// <summary>
    /// Converts a regular node index to a grid index where 0 is row and 1 is column
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    int[] ConvertIndexToGridIndex(int index)
    {
        int[] gIndex = new int[2];
        gIndex[0] = Mathf.FloorToInt((float)index / (float)_width);
        gIndex[1] = index - (gIndex[0] * _width);

        return gIndex;
    }
}

/// <summary>
/// A* node
/// </summary>
public class ANode
{
    public int[] gIndex; //is an array of the row ([0]) column ([1]) index
    public Vector3 Position; // Is used to find the nearest node and to find where to go
    public ANode Parent; // The parenting structure used to retrace the found path
    public float H; //The estimate from this node to target node
    public float G; //The value required to travel to this node from start
    public bool Available = true; //Should this node be included in the search

    public float F
    {
        get { return H + G; }
    }
}
