using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class GroundSpring : MonoBehaviour
{
    Transform tr;
    Rigidbody rb;
    Collider col;
    RaycastSensor groundSensor;
    [Header("Spring Distances")]
    public float groundRayLength = 3f;
    public float restDistance = 2f;
    [Header("Spring Properties")]
    public float springStrength = 100f;
    public float springDampening = 100f;
    
    Vector3 rayStartPosition;
    
    public bool IsGrounded() =>  groundSensor.HasDetectedHit();
    public Vector3 GroundNormal() =>  groundSensor.GetNormal();

    public void AwakeGroundSpring()
    {
        tr = transform;
        rb = tr.GetComponent<Rigidbody>();
        col = tr.GetComponent<Collider>();
        rayStartPosition = col.bounds.center;
    }
   
    void OnDrawGizmos()
    { 
        AwakeGroundSpring();
        using (new Handles.DrawingScope(Color.red))
        {
            Handles.DrawLine(rayStartPosition, rayStartPosition + groundRayLength * -tr.up, 5f);
        }
        using (new Handles.DrawingScope(Color.green))
        {
            Handles.DrawLine(rayStartPosition + Vector3.forward, rayStartPosition + restDistance * -tr.up + Vector3.forward, 5f);
        }
    }

    public void CheckForGround()
    {
        SetupGroundSensor();
        
        groundSensor.Cast();

        if (IsGrounded())
        {
            ApplySpringForce();
            Debug.DrawLine(rayStartPosition, groundSensor.GetPosition(), Color.green);
        }
        else
        {
            Debug.DrawLine(rayStartPosition, rayStartPosition + groundRayLength * -tr.up, Color.red);
        }
        
    }
    public void ApplySpringForce()
    {
        //get velocity in the downward direction
        float downwardVelocity = Vector3.Dot(rb.velocity, -tr.up);
        
        //get how much our spring has been offset from our rest distance
        float springOffset = groundSensor.GetDistance() - restDistance;
        
        //calculate the force from our strength and dampening
        float force = (springOffset * springStrength * 100f) - (downwardVelocity * springDampening * 10f);
        
        //add this force in the downward direction (the force itself will be negative if we need to go up)
        rb.AddForce(-tr.up * force);
    }
    void SetupGroundSensor()
    {
        //null assigment operator, if we dont have a sensor, make one, else use the one that exists
        groundSensor ??= new RaycastSensor(tr);
        
        groundSensor.SetCastOrigin(rayStartPosition);
        groundSensor.SetCastDirection(RaycastSensor.CastDirection.Down);
        groundSensor.castLength = groundRayLength * tr.localScale.x;
    }
}
