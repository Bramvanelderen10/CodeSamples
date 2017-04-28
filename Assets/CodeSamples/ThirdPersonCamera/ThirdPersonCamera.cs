using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// Orbits around the selected target based on values in CameraData
/// </summary>
public class ThirdPersonCamera : AbstractCamera
{
    private ThirdPersonCameraData _data;
    private float _lastInput = 0;
    private Vector3 _currentEuler = Vector3.zero;
    private Vector3 _position = Vector3.zero;
    private Transform _target;


    // Use this for initialization
    void Start ()
	{
	    _data = Resources.Load<ThirdPersonCameraData>("ThirdPersonCameraData");
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (!_target)
            return;

        var h = InputWrapper.GetAxis(_data.Horizontal.Axis) * (_data.Horizontal.inverted ? -1 : 1);
        var v = InputWrapper.GetAxis(_data.Vertical.Axis) * (_data.Vertical.inverted ? -1 : 1);
        if (h != 0 || v != 0)
        {
            _currentEuler.x += _data.RotationSpeedInput.x * v;
            _currentEuler.y += _data.RotationSpeedInput.y * h;
            

            transform.eulerAngles = _currentEuler;
            _lastInput = Time.time;
        }

        if (Time.time - _lastInput > _data.DisableRotationAfterInput)
	    {
	        transform.rotation = Quaternion.RotateTowards(transform.rotation, _target.rotation * Quaternion.Euler(_data.OffsetRotation),
                _data.RotationSpeed.x*Time.deltaTime);
	        _currentEuler = transform.eulerAngles;
        }

	    _position = _target.position;
        Vector3 position = _position + (transform.rotation * _data.OffsetPosition);
        var ray = new Ray(_target.position, (position - _target.position).normalized);
        foreach (var hit in Physics.RaycastAll(ray, Vector3.Distance(_target.position, position)))
	    {
	        if (hit.transform != _target && hit.transform != transform && !_data.IgnoreTags.Contains(hit.transform.tag))
	        {
	            if (Vector3.Distance(_target.position, hit.point) < Vector3.Distance(_target.position, position))
	            {
	                position = hit.point;
	            }
	        }
	    }
	    transform.position = Vector3.MoveTowards(transform.position, position, _data.MoveSpeed*Time.deltaTime);
	}

    public override void AddTarget(Transform target)
    {
        if (target.CompareTag("Player"))
            _target = target;
    }
}
