using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThirdPersonCameraData", menuName = "Data/ThirdPersonCameraData")]
public class ThirdPersonCameraData : ScriptableObject
{
    public List<string> IgnoreTags = new List<string>();
    public Vector3 OffsetPosition = new Vector3(0f, 1.5f, -10f);
    public float MoveSpeed = 50f;
    public Vector3 OffsetRotation = new Vector3(10f, 0f, 0f);
    public Vector3 RotationSpeed = new Vector3(300f, 0f, 0f);
    public Vector3 RotationSpeedInput = new Vector3(4f, 4f, 0f);
    public Vector3 RotationLimit = new Vector3(80f, 360, 0);
    public float DisableRotationAfterInput = .1f;
    public AxisData Horizontal;
    public AxisData Vertical;

    [System.Serializable]
    public struct AxisData
    {
        public InputAxis Axis;
        public bool inverted;
    }
}
