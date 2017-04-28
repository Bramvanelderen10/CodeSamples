using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "IsometricCameraData", menuName = "Data/IsometricCameraData")]
public class IsometricCameraData : ScriptableObject
{
    public float MoveSpeed = 4f;
    public float ZoomOutSpeed = 4f;
    public float ZoomInSpeed = .6f;
    public float Treshold = .1f;
    public float MinSize = 25f;
    public float ScaleOffset = 3.5f;
    public float xRotation = 80f;
}
