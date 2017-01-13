using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
This script will position and resize the camera to optimally display all the action happening in an top down game
This is used in Tribot Smash!
https://www.youtube.com/watch?v=DBWMmZ5GOdQ
*/
public class CameraFollow : MonoBehaviour
{
    public float MoveSpeed = 4f;
    public float ResizeSpeed = 4f;
    public float Treshold = 0.1f;
    public float MinSize = 5f;
    public float ScaleOffset = 1f; //Scaleoffset is applied after the camera is scaled to fit all targets. The screensize will be expanded by this value

    public List<string> FollowTags = new List<string>
    {
        "Player",
        "PickUp"
    };

    private Camera _camera;
    private List<GameObject> _targetObjects = new List<GameObject>();
    
	// Use this for initialization
	void Start ()
	{
	    _camera = Camera.main;
        
	}
	
	// Update is called once per frame
	void Update ()
	{
	    CleanUpObjects(); //Remove all empty objects from the list
        //Add all objects with specified tags to the target list
	    foreach (var item in FollowTags)
	    {
            foreach (var obj in GameObject.FindGameObjectsWithTag(item))
            {
                AddObject(obj);
            }
        }

	    if (_targetObjects.Count < 1)
	        return;
        
        //Determine rectangle around the targets
        var minX = 0f;
        var maxX = 0f;
        var minZ = 0f;
        var maxZ = 0f;

        for (var i = 0; i < _targetObjects.Count; i++)
        {
            var pos = _targetObjects[i].transform.position;

            if (i == 0)
            {
                minX = pos.x;
                maxX = pos.x;
                minZ = pos.z;
                maxZ = pos.z;
            }
            else
            {
                if (pos.x > maxX)
                {
                    maxX = pos.x;
                }
                if (pos.x < minX)
                {
                    minX = pos.x;
                }

                if (pos.z > maxZ)
                {
                    maxZ = pos.z;
                }
                if (pos.z < minZ)
                {
                    minZ = pos.z;
                }
            }
        }

	    var playerCorners = new Corners();
        playerCorners.DownLeft = new Vector3(minX, 0, minZ);
        playerCorners.DownRight = new Vector3(maxX, 0, minZ);
        playerCorners.UpLeft = new Vector3(minX, 0, maxZ);
        playerCorners.UpRight = new Vector3(maxX, 0, maxZ);

        //Calculate the center position between the targets and lerp the camera position towards the center
        var center = new Vector3((minX + maxX) / 2, transform.position.y, (minZ + maxZ) / 2);
	    transform.position = Vector3.Lerp(transform.position, center, MoveSpeed * Time.deltaTime);


        //Calculate optimal camera size based on targets
        //Determine world position of all camera corners on a invisible plane
	    var cameraCorners = new Corners();
	    var plane = new Plane(Vector3.up, Vector3.zero);
	    float distance;
	    var ray = _camera.ViewportPointToRay(Vector3.zero);
	    if (plane.Raycast(ray, out distance))
	    {
	        cameraCorners.DownLeft = ray.GetPoint(distance);
	    }

	    ray = _camera.ViewportPointToRay(Vector3.up);
	    if (plane.Raycast(ray, out distance))
	    {
	        cameraCorners.UpLeft = ray.GetPoint(distance);
	    }

        ray = _camera.ViewportPointToRay(Vector3.right);
        if (plane.Raycast(ray, out distance))
        {
            cameraCorners.DownRight = ray.GetPoint(distance);
        }

        ray = _camera.ViewportPointToRay(Vector3.one);
        if (plane.Raycast(ray, out distance))
        {
            cameraCorners.UpRight = ray.GetPoint(distance);
        }

        //Determine with which value the camera size has to be multiplied to fit all targets in the screen
	    var scaleValues = new List<float>();
	    foreach (var target in _targetObjects)
	    {
	        var targetX = target.transform.position.x;
            var targetZ = target.transform.position.z;

            //The calculate below can be explained as follows
            //Compare the minimum and maximum values on a certain axis to the value of the target
            //determine which value the camera size has to be multiplied with to get this point with the min and max axis value of the camera
            //This is done 4 times for the min and max values of the camera for both the axis x and z
            var tempScale = ((((ScaleOffset + targetX) - cameraCorners.MaxX) * 2) + cameraCorners.GetWidth()) / cameraCorners.GetWidth();
	        scaleValues.Add(tempScale);

            tempScale = (((cameraCorners.MinX - (targetX - ScaleOffset)) * 2) + cameraCorners.GetWidth()) / cameraCorners.GetWidth();
            scaleValues.Add(tempScale);

            tempScale = (((cameraCorners.MinZ - (targetZ - ScaleOffset)) * 2) + cameraCorners.GetHeight()) / cameraCorners.GetHeight();
            scaleValues.Add(tempScale);

            tempScale = ((((targetZ + ScaleOffset) - cameraCorners.MaxZ) * 2) + cameraCorners.GetHeight()) / cameraCorners.GetHeight();
            scaleValues.Add(tempScale);
        }
        //Get the optimal determined value
	    var max = scaleValues.Max(x => x);

        //Lerp camera towards optimal screen size only within the certain threshold
	    if (max > 1f || max < (1f - Treshold))
	    {
	        var target = _camera.orthographicSize * max;
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, target, ResizeSpeed*Time.deltaTime);
	    }

        //Camera can't scale below minimum size
	    if (_camera.orthographicSize < MinSize)
	    {
	        _camera.orthographicSize = MinSize;
	    }
    }

    //Remove all empty targets
    void CleanUpObjects()
    {
        _targetObjects.RemoveAll(p => p == null);
    }

    //Add new target
    void AddObject(GameObject obj)
    {
        if (_targetObjects.Contains(obj))
            return;

        _targetObjects.Add(obj);
    }

    private class Corners
    {
        public Vector3 DownLeft;
        public Vector3 DownRight;
        public Vector3 UpLeft;
        public Vector3 UpRight;

        public float MinX
        {
            get { return (DownLeft.x >= UpLeft.x) ? UpLeft.x : DownLeft.x; }
        }

        public float MaxX
        {
            get { return (DownRight.x >= UpRight.x) ? UpRight.x : DownRight.x; }
        }

        public float MinZ
        {
            get { return (DownLeft.z >= DownRight.z) ? DownRight.z : DownLeft.z; }
        }

        public float MaxZ
        {
            get { return (UpLeft.z >= UpRight.z) ? UpRight.z : UpLeft.z; }
        }

        public float GetWidth()
        {

            return MaxX - MinX;
        }

        public float GetHeight()
        {

            return UpLeft.z - DownLeft.z;
        }

    }
}
