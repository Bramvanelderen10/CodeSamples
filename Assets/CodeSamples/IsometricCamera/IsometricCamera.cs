using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/*
This script will position and resize the camera to optimally display all the action happening in an top down game
This is used in Tribot Smash!
https://www.youtube.com/watch?v=DBWMmZ5GOdQ
*/
public class IsometricCamera : AbstractCamera
{
    private Camera _camera;
    private List<GameObject> _targetObjects = new List<GameObject>();
    private float _minX = 0f;
    private float _maxX = 0f;
    private float _minZ = 0f;
    private float _maxZ = 0f;
    private Vector3 _localPosition = new Vector3(0, 0, -20);
    private IsometricCameraData _data;
    private Vector3 _position = new Vector3(0, 0, 0);

    // Use this for initialization
    void Start()
    {
        _data = Resources.Load<IsometricCameraData>("IsometricCameraData");
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(_data.xRotation, 0, 0);
        CleanUpObjects();

        if (_targetObjects.Count < 1)
            return;
        var rotation = transform.rotation*Quaternion.Inverse(Quaternion.Euler(0, 90, 0));
        for (var i = 0; i < _targetObjects.Count; i++)
        {
            var pos = _targetObjects[i].transform.position;

            //Always use the first index as default values so the camera scales and follows correctly
            if (i == 0)
            {
                _minX = pos.x;
                _maxX = pos.x;
                _minZ = pos.z;
                _maxZ = pos.z;
            }
            else
            {
                _maxX = Mathf.Max(_maxX, pos.x);
                _minX = Mathf.Min(_minX, pos.x);
                _maxZ = Mathf.Max(_maxZ, pos.z);
                _minZ = Mathf.Min(_minZ, pos.z);
            }
        }

        //Calculate the center position between the targets and lerp the camera position towards the center
        var center = new Vector3((_minX + _maxX) / 2, _position.y, (_minZ + _maxZ) / 2);
        _position = Vector3.Lerp(_position, center, _data.MoveSpeed * Time.deltaTime);

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
            var tempScale = ((((_data.ScaleOffset + targetX) - cameraCorners.RightHorizontalValue) * 2) + cameraCorners.GetWidth()) / cameraCorners.GetWidth();
            scaleValues.Add(tempScale);

            tempScale = (((cameraCorners.LeftHorizontalValue - (targetX - _data.ScaleOffset)) * 2) + cameraCorners.GetWidth()) / cameraCorners.GetWidth();
            scaleValues.Add(tempScale);

            tempScale = (((cameraCorners.BottomVerticalValue - (targetZ - _data.ScaleOffset)) * 2) + cameraCorners.GetHeight()) / cameraCorners.GetHeight();
            scaleValues.Add(tempScale);

            tempScale = ((((targetZ + _data.ScaleOffset) - cameraCorners.TopVecticalValue) * 2) + cameraCorners.GetHeight()) / cameraCorners.GetHeight();
            scaleValues.Add(tempScale);
        }
        //Get the optimal determined value
        var max = scaleValues.Max(x => x);

        //Lerp camera towards optimal screen size only within the certain threshold
        if (max > 1f || max < (1f - _data.Treshold))
        {
            var target = _localPosition.z * max;
            var speed = (target > _localPosition.z) ? _data.ZoomInSpeed : _data.ZoomOutSpeed;
            _localPosition.z = Mathf.Lerp(_localPosition.z, target, speed * Time.deltaTime);

        }

        //Camera can't scale below minimum size
        if (_localPosition.z > -_data.MinSize)
        {
            _localPosition.z = -_data.MinSize;
        }

        _camera.transform.position = _position + (transform.rotation*_localPosition);
    }

    /// <summary>
    /// Remove all empty targets
    /// </summary>
    void CleanUpObjects()
    {
        _targetObjects.RemoveAll(p => p == null);
    }

    /// <summary>
    /// Adds new target to the target list
    /// </summary>
    /// <param name="obj"></param>
    public override void AddTarget(Transform obj)
    {
        if (_targetObjects.Contains(obj.gameObject))
            return;

        _targetObjects.Add(obj.gameObject);
    }

    /// <summary>
    /// Holds information about camera corners and differences
    /// </summary>
    private class Corners
    {
        public Vector3 DownLeft;
        public Vector3 DownRight;
        public Vector3 UpLeft;
        public Vector3 UpRight;

        /// <summary>
        /// Returns the smallest left side X value
        /// </summary>
        public float LeftHorizontalValue
        {
            get { return Mathf.Max(DownLeft.x, UpLeft.x); }
        }

        /// <summary>
        /// Returns the biggest right side X value
        /// </summary>
        public float RightHorizontalValue
        {
            get { return Mathf.Min(DownRight.x, UpRight.x); }
        }

        /// <summary>
        /// Returns the smallest bottom side Z value
        /// </summary>
        public float BottomVerticalValue
        {
            get { return Mathf.Max(DownLeft.z, DownRight.z); }
        }

        /// <summary>
        /// Returns the biggest top side Z value
        /// </summary>
        public float TopVecticalValue
        {
            get { return Mathf.Min(UpLeft.z, UpRight.z); }
        }

        public float GetWidth()
        {

            return RightHorizontalValue - LeftHorizontalValue;
        }

        public float GetHeight()
        {

            return UpLeft.z - DownLeft.z;
        }

    }
}
