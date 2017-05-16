using UnityEngine;

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
    
    void Start ()
	{
	    _data = Resources.Load<ThirdPersonCameraData>("ThirdPersonCameraData");
	}

    void LateUpdate ()
    {
        if (!_target)
            return;

        var h = InputWrapper.GetAxis(_data.Horizontal.Axis) * (_data.Horizontal.inverted ? -1 : 1);
        var v = InputWrapper.GetAxis(_data.Vertical.Axis) * (_data.Vertical.inverted ? -1 : 1);

        //If the user is inputing something record that input
        if (h != 0 || v != 0)
        {
            _currentEuler.x += _data.RotationSpeedInput.x * v;
            _currentEuler.y += _data.RotationSpeedInput.y * h;
            

            transform.eulerAngles = _currentEuler;
            _lastInput = Time.time;
        }

        //If the user hasn't done anything for a short while automatically rotate the camera towards the target rotation
        if (Time.time - _lastInput > _data.DisableRotationAfterInput)
	    {
	        transform.rotation = Quaternion.RotateTowards(transform.rotation, _target.rotation * Quaternion.Euler(_data.OffsetRotation),
                _data.RotationSpeed.x*Time.deltaTime);
	        _currentEuler = transform.eulerAngles;
        }

	    _position = _target.position;
        Vector3 position = _position + (transform.rotation * _data.OffsetPosition); //Calculate new camera position
        var ray = new Ray(_target.position, (position - _target.position).normalized);
        //Check if there is anything between the camera and the target
        //If so move the camera in between the 2 so the target stays visible
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
        //Move the camera to the new position
	    transform.position = Vector3.MoveTowards(transform.position, position, _data.MoveSpeed*Time.deltaTime);
	}

    /// <summary>
    /// Add new target to the camera, Can only be a object tagged with player
    /// </summary>
    /// <param name="target"></param>
    public override void AddTarget(Transform target)
    {
        if (target.CompareTag("Player"))
            _target = target;
    }
}
