using UnityEngine;

[System.Serializable]
public class Axle
{
    public WheelRay leftWheel;
    public WheelRay rightWheel;

    public AxleLocation axleLocation;
    public Transform leftWheelTransform;
    public Transform rightWheelTransform;
    public bool steering;
    public bool motor;
  

    public enum AxleLocation {Front,Back}
}


