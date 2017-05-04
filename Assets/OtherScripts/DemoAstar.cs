using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Astar))]
public class DemoAstar : MonoBehaviour
{
    [SerializeField] private GameObject _visualNodePrefab;

    private Astar _astar;
    private List<ANode> _path;

    private List<GameObject> _visualPath = new List<GameObject>();

    [HideInInspector]
    public int _rows = 10;
    [HideInInspector]
    public int _columns = 10;
    [HideInInspector]
    public bool ContinuesPathDemo = true;
    [HideInInspector]
    public Vector3 StartPosition = Vector3.zero;
    [HideInInspector]
    public Vector3 TargetPosition  = Vector3.one * 10;

    [HideInInspector] public GameObject StartObj;
    [HideInInspector] public GameObject EndObj;

    [HideInInspector] public bool TargetBased = false;

    private ObjectPool _pool;
    

	// Use this for initialization
	void Start ()
	{
	    _pool = new ObjectPool(_visualNodePrefab, 500, true);
	    _astar = GetComponent<Astar>();
	    GenerateGrid();
	    GeneratePath();
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void GenerateGrid()
    {
        _astar.GenerateGrid(_columns, _rows);
    }

    public void GeneratePath()
    {
        var start = StartPosition;
        var end = TargetPosition;
        if (TargetBased)
        {
            start = StartObj.transform.position;
            end = EndObj.transform.position;
        }

        _astar.GetPath(start, end, SavePath);
    }

    public void DisplayPath()
    {
        CleanUp();
        if (_path == null)
            return;
        foreach (var node in _path)
        {
            var obj = _pool.GetObject();
            obj.transform.position = node.Position;
            _visualPath.Add(obj);
        }
    }

    void SavePath(List<ANode> path)
    {
        //print("Path found!");
        _path = path;

        if (ContinuesPathDemo)
        {
            DisplayPath();
            GeneratePath();
        }
    }

    public void CleanUp()
    {
        foreach (var item in _visualPath)
        {
            item.SetActive(false);
        }
    }
}
