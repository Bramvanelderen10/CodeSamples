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
    public bool ContinuesPathDemo = true;
    [HideInInspector]
    public Vector3 StartPosition = Vector3.zero;
    [HideInInspector]
    public Vector3 TargetPosition  = Vector3.one * 10;

    [HideInInspector] public GameObject StartObj;
    [HideInInspector] public GameObject EndObj;

    [HideInInspector] public bool TargetBased = false;

    

	// Use this for initialization
	void Start ()
	{
	    _astar = GetComponent<Astar>();
	    GenerateGrid(15, 15);
	    GeneratePath();
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void GenerateGrid(int rows, int columns)
    {
        _astar.GenerateGrid(columns, rows);
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
            var obj = Instantiate(_visualNodePrefab);
            obj.transform.position = node.Position;
            _visualPath.Add(obj);
        }
    }

    void SavePath(List<ANode> path)
    {
        print("Path found!");
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
            Destroy(item);
        }
    }
}
