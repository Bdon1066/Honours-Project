using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSensor
{
    public float castLength = 1.0f;
    public LayerMask layermask = 255;

    Vector3 origin = Vector3.zero;
    Transform tr;

    public enum CastDirection {Forward, Right, Up, Backward, Left, Down }
    CastDirection castDirection;
}
