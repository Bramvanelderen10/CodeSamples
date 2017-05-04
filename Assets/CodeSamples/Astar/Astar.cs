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

    [SerializeField] private List<string> _hitTags = new List<string>();
    [SerializeField] private Vector3 _nodeHalfExtends;
    private List<ANode> _nodes;
    private ANode[,] _nodesArray;

    private bool _isSearching = false;

    private int _width;
    private int _length;

    private ActionPath _actionPath;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update () {
		
	}

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
                node.Index = j + (width * i);
                node.Position = new Vector3(j + (_nodeHalfExtends.x), transform.position.y, i + (_nodeHalfExtends.z));
                node.Walkable = false;
                node.gIndex = new int[2];
                node.gIndex[0] = i;
                node.gIndex[1] = j;
                _nodesArray[i, j] = node;
            }
        }
    }

    public Vector3 FindWhereToGo(List<ANode> path, Vector3 current)
    {
        var currentNode = FindNearestNode(path, current);

        int index = path.FindIndex(x => x == currentNode);
        if (index < path.Count - 1)
            index++;

        return path[index].Position;
    }

    public void GetPath(Vector3 startPosition, Vector3 targetPosition, ActionPath action)
    {
        if (_isSearching)
            return;

        _actionPath = action;
        StartCoroutine(FindPath(.2f, FindNearestNode(startPosition), FindNearestNode(targetPosition)));
    }

    IEnumerator FindPath(float maxDuration, ANode start, ANode target)
    {
        _isSearching = true;
        List<ANode> _openNodes = new List<ANode>();
        List<ANode> _closedNodes = new List<ANode>();

        List<ANode> _testNodes = new List<ANode>();


        foreach (var node in _nodesArray)
        {
            _testNodes.Add(node);
            node.Walkable = false;
            node.InOpen = false;
            node.InClosed = false;
            ///Maybe use layermasks instead to simplify this loop
            var hits = Physics.OverlapBox(node.Position, _nodeHalfExtends);
            bool hit = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (_hitTags.Contains(hits[i].tag))
                {
                    hit = true;
                    i = hits.Length;
                }
            }
            if (hit)
                continue;

            //convert index back to row and column index
            var nodeGIndex = ConvertIndexToGridIndex(node.Index);
            var targetGIndex = ConvertIndexToGridIndex(target.Index);
            //Calculate h value CAN BE BACKED IN GENERATEGRID
            node.H = Mathf.Abs(nodeGIndex[1] - targetGIndex[1]) + Mathf.Abs(nodeGIndex[0] - targetGIndex[0]);
            node.G = 0;
            node.Walkable = true;

            //_openNodes.Add(node);
        }
        _openNodes.Add(start);
        start.InOpen = true;

        float waitTime = maxDuration/_nodesArray.Length;

        while (_openNodes.Count != 0)
        {
            var current = _openNodes.Aggregate((x1, x2) => x1.F < x2.F ? x1 : x2);

            if (current == target)
            {
                break;
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
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
                    if (!node.Walkable || node.InOpen || node.InClosed)
                        continue;

                    if (i == -1 && j == -1 ||
                        i == -1 && j == 1 ||
                        i == 1 && j == -1 ||
                        i == 1 && j == 1)
                        node.G = current.G + 14;
                    else
                        node.G = current.G + 10;

                    node.Parent = current;
                    node.InOpen = true;
                    _openNodes.Add(node);
                }
            }
            _openNodes.Remove(current);
            current.InOpen = false;
            current.InClosed = true;
            _closedNodes.Add(current);

            yield return null;
        }
        _isSearching = false;
        _actionPath(RetracePath(start, target));
    }

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

    int[] ConvertIndexToGridIndex(int index)
    {
        int[] gIndex = new int[2];
        gIndex[0] = Mathf.FloorToInt((float)index / (float)_width);
        gIndex[1] = index - (gIndex[0] * _width);

        return gIndex;
    }
}

public class ANode
{
    public int Index;
    public int[] gIndex; //is an array of the row ([0]) column ([1]) index

    public Vector3 Position;

    public ANode Parent;
    public float H;
    public float G;
    public bool Walkable;
    public bool InOpen = true;
    public bool InClosed = true;


    public float F
    {
        get { return H + G; }
    }
}
