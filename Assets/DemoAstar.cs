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

	// Use this for initialization
	void Start ()
	{
	    _astar = GetComponent<Astar>();
	    GenerateGrid(15, 15);
	    GeneratePath(new Vector3(0, 0, 0), new Vector3(12, 0, 12));
	    DisplayPath();
	}
	
	// Update is called once per frame
	void Update () {
        DisplayPath();
    }

    public void GenerateGrid(int rows, int columns)
    {
        _astar.GenerateGrid(columns, rows);
    }

    public void GeneratePath(Vector3 start, Vector3 end)
    {
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
        _path = path;
    }

    void CleanUp()
    {
        foreach (var item in _visualPath)
        {
            Destroy(item);
        }
    }
}
