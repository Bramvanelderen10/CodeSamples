using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
This class generates meshes that are used as level in a game where you place as ball
Using the procedural mesh generation in combination with procedural level generation would result in unique enviroments
 */

public class LandscapeGenerator : MonoBehaviour {

    public struct LastMeshInfo
    {
        public Vector3 position;
        public Vector3 endPoint;

        public LastMeshInfo(Vector3 position, Vector3 endPoint) : this()
        {
            this.position = position;
            this.endPoint = endPoint;
        }
    }

    public int _nodeInterval = 5;
    public float _railHeight = 3f;

    public float _distance = 250f;
    private int _length = 100, _width = 10;
    private float _curveDepth = 0f, _curveLength = 0f, _steepness = 2f;
    

    private LastMeshInfo _lastMeshInfo;
    private System.Random rnd = new System.Random();

	// Use this for initialization
	void Start () {
        _lastMeshInfo = new LastMeshInfo(transform.position, new Vector3(0, 0, 0));

        RandomizeMesh();
        CreateMesh(_length, _width, _curveDepth, _curveLength, _steepness);
        RandomizeMesh();
        CreateMesh(_length, _width, _curveDepth, _curveLength, _steepness);
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 target = _lastMeshInfo.position + _lastMeshInfo.endPoint;
        Vector3 position = Player.Instance.transform.position;

        if (Vector3.Distance(position, target) < _distance) 
        {
            RandomizeMesh(); //Randomize mesh parameters so a unique mesh can be created
            CreateMesh(_length, _width, _curveDepth, _curveLength, _steepness);
        }
    }

    void CreateMesh(int length, int width, float curveDepth = 0f, float curveLength = 0f, float steepness = 1f)
    {
        GameObject landscapeObject = ObjectPooler.Instance.GetPooledObject("Landscape"); //Retrieve a gameobject with empty mesh from the object pooler
        if (!landscapeObject)
            return;

        Vector3 position = new Vector3();
        position = _lastMeshInfo.position + _lastMeshInfo.endPoint; //Determine start position of the new mesh based on the previous mesh start and end position
        landscapeObject.transform.position = position;        

        landscapeObject.SetActive(true);
        landscapeObject.tag = "env";
        MeshFilter mf = landscapeObject.GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        int totalVertices = (length + 1) * (width + 1);

        /*
        Find edge numbers
        */
        List<int> vertexNumbersLeft = new List<int>();
        List<int> vertexNumbersRight = new List<int>();

        for (int i = 0; i < length + 1; i++)
        {
            vertexNumbersLeft.Add(i);
            vertexNumbersRight.Add(i + (width * (length + 1)));
        }

        List<int> vertexNumbersTop = new List<int>();
        List<int> vertexNumbersBot = new List<int>();

        for (int i = 0; i < width + 1; i++)
        {
            vertexNumbersBot.Add(i * (length + 1));
            vertexNumbersTop.Add((i * (length + 1)) + length);
        }

        int vertexNumber = 0;
        Vector3[] vertices = new Vector3[totalVertices];

        /*
        Calculate middle of the plane
        */
        int mid = width / 2;
                
        /*
        Loop through the width of the plane then in each loop generate a full row in length of vertices
        */
        for (int w = 0; w < width + 1; w++)
        {
            int lengthCounter = 0;
            for (int l = 0; l < length + 1; l++)
            {
                int random = rnd.Next(0, 1);
                if (random == 0)
                    random = -1;
                //float x = w; //change to sin(l) + w for curve
                float x = w + ((Mathf.Sin((float)l / (float)curveLength) * random) * curveDepth); //Adds a curve to the rectangle mesh
                float temp_steep = (-((float)l * steepness) / 4f); //Adds a steepness to the mesh
                float parabooleCurve = Mathf.Pow((float)w - (((float)width + 1f) / 2f), 2f) / 24f; //Adds a vertical paraboole to the mesh
                float y = 0 + parabooleCurve;
                float z = l;

                vertices[vertexNumber] = new Vector3(x, y, z);

                /*
                Save last point to place the next generated mesh on the correct position
                */
                if (vertexNumbersLeft[vertexNumbersLeft.Count - 1] == vertexNumber)
                {
                    _lastMeshInfo.endPoint = vertices[vertexNumber];
                    _lastMeshInfo.endPoint.y = 0; // add steepness if you want downcuve
                    _lastMeshInfo.position = landscapeObject.transform.position;
                }
                /*
                Place rail nodes
                The rail nodes are used by the camera, the camera will follow a path created by these nodes
                */   
                if (w == mid)
                {
                    if (lengthCounter == 0)
                    {
                        Vector3 node = _lastMeshInfo.position;
                        Vector3 adjustment = vertices[vertexNumber];
                        //adjustment.y = ((-((float)l * steepness) / 4f)) + _railHeight;
                        adjustment.y += _railHeight;
                        node += adjustment;

                        Rail.Instance.AddNode(node);
                    }
                    lengthCounter++;
                    if (lengthCounter == _nodeInterval)
                    {
                        lengthCounter = 0;
                    }
                }

                vertexNumber++;
            }
        }        

        /*
        This code combines the vertices into triangles
        */
        int[] triangles = new int[((width * length) * 2) * 3]; 
        int triangleNumber = 0;
        for (int i = 0; i < totalVertices; i++)
        {

            /*
            If no vertices above the current vertex so no more triangles
            */
            if (vertexNumbersTop.Contains(i))
                continue;

            /*
            Create triangle to the left of the current vertex
            */
            if (!vertexNumbersLeft.Contains(i))
            {
                triangles[triangleNumber] = i;
                triangleNumber++;
                triangles[triangleNumber] = i - (length + 1);
                triangleNumber++;
                triangles[triangleNumber] = i + 1;
                triangleNumber++;
            }

            /*
            Create triangle to the right of the current vertex
            */
            if (!vertexNumbersRight.Contains(i))
            {
                triangles[triangleNumber] = i;
                triangleNumber++;
                triangles[triangleNumber] = i + 1;
                triangleNumber++;
                triangles[triangleNumber] = i + (length + 1) + 1;
                triangleNumber++;                
            }
        }

        /*
        Save all information into the mesh and execute optimize and recalculatenormals methods
        */
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.Optimize();

        mf.mesh = mesh;
        landscapeObject.GetComponent<MeshCollider>().sharedMesh = mf.mesh;
    }


    //Randomizes all parameters that are used by the mesh generation to create a unique mesh each time
    void RandomizeMesh()
    {
        _length = (int)Random.Range(50, 200);
        _width = 24;
        //_curveDepth = Random.Range(0, 50f);
        //if (_lastMeshInfo.endPoint.x < 0f) {
        //    _curveDepth *= -1;
        //}

        //int random = rnd.Next(0, 1);
        //if (random == 0)
        //    random = -1;
        //_curveLength = random * _curveDepth * Random.Range(1f, 4f);

        if (_lastMeshInfo.endPoint.x >= 0f)
        {
            _curveDepth = Random.Range(0f, 100f);
            _curveLength = 90f;
        }

        if (_lastMeshInfo.endPoint.x < 0f)
        {
            _curveDepth = Random.Range(0f, -100f);
            _curveLength = 90f;
        }
        _steepness = Random.Range(1.7f, 2.1f);
    }
}
