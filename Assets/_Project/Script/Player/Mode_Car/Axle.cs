using UnityEngine;

[System.Serializable]
public class Axle
{
    public WheelRay leftWheel;
    public WheelRay rightWheel;
    
    public Transform leftWheelTransform;
    public Transform rightWheelTransform;
    public bool steering;
    public bool motor;
}


