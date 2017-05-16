using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the flight controller is responsible for applying player input to the target
/// Isometric and third person camera's are supported
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FlightController : MonoBehaviour
{
    enum FlightMode
    {
        Isometric,
        ThirdPerson
    }

    [SerializeField] private FlightMode _mode;
    [SerializeField] private float _speed;
    [SerializeField] private float _elevationSpeed = 10f;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private InputAxis _horizontal;
    [SerializeField] private InputAxis _vertical;
    [SerializeField] private InputAxis _ascend;
    [SerializeField] private InputAxis _descend;
    [SerializeField] private float _lostPlayerControlDuration = .5f;

    private Rigidbody _rb;
    private Vector3 _input;
    private Vector3 _elevation = Vector3.zero;
    private bool _playerControl = true;

    /// <summary>
    /// Rotationspeed differs in the different perspective modes
    /// </summary>
    private float RotationSpeed
    {
        get { return _mode == FlightMode.Isometric ? _rotationSpeed*2 : _rotationSpeed; }
    }


    /// <summary>
    /// On start add itself to the camera target and listen to bumps
    /// </summary>
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        var coll = GetComponent<FlightCollision>();
        if (coll)
            coll.AddBumpListener(BumpListener);

        foreach (var camera in (AbstractCamera[]) FindObjectsOfType(typeof(AbstractCamera)))
        {
            camera.AddTarget(transform);
        }
    }

    /// <summary>
    /// Save input and apply rotation to player
    /// </summary>
    void Update()
    {
        //Save input each frame no matter what
        _elevation = new Vector3(0, InputWrapper.GetAxis(_ascend, true) - InputWrapper.GetAxis(_descend, true), 0);
        _input = new Vector3(InputWrapper.GetAxis(_horizontal), 0, InputWrapper.GetAxis(_vertical));

        if (_playerControl && (_input.x != 0 || _input.z != 0))
        {
            var targetRotation = Quaternion.identity;
            switch (_mode)
            {
                case FlightMode.Isometric:
                    targetRotation = Quaternion.Euler(0, -1 * (Mathf.Atan2(_input.z, _input.x) * Mathf.Rad2Deg - 90), 0f); //Angle based on input
                    break;
                case FlightMode.ThirdPerson:
                    targetRotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0f); //Angle based on camera
                    break;
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Apply input to velocity
    /// </summary>
    void FixedUpdate()
    {
        if (!_playerControl)
            return;

        var vel = _rb.velocity;
        if (_input.x != 0 || _input.z != 0 || _elevation.y != 0)
        {
            var direction = Vector3.ClampMagnitude(_input, 1f); //Clamp input for correct speed
            vel = (_mode == FlightMode.ThirdPerson
                ? transform.rotation
                : Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f))*direction*_speed;
            vel += _elevation * _elevationSpeed;
        }
        else
        {
            vel = Vector3.zero;
        }
        _rb.velocity = vel;
        _rb.angularVelocity = Vector3.zero;
    }

    private void BumpListener()
    {
        StopAllCoroutines();
        StartCoroutine(LosePlayerControl());
    }

    IEnumerator LosePlayerControl()
    {
        _playerControl = false;
        yield return new WaitForSeconds(_lostPlayerControlDuration);
        _playerControl = true;
    }
}
