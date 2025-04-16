using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GroundSpring : MonoBehaviour
{
    Transform tr;
    Rigidbody rb;
    Collider col;
    RaycastSensor groundSensor;
    [Header("Spring Properties")]
    public Transform originTransform;
    public CastDirection springDirection;
    public Vector3 originOffset;
    public bool useSphereCast;
    public float sphereRadius;
    public LayerMask layerMask;
    [Header("Spring Length")]
    public float springLength = 3.2f;
    public float restDistance = 2.5f;
    [Header("Spring Strength")]
    public float springStrength = 100f;
    public float springDampening = 100f;
    
    Vector3 rayStartPosition;

    public bool extendSensor = true;
    public bool    InContact()       =>  groundSensor.HasDetectedHit() && enableSpring;
    public Vector3 ContactNormal()   =>  groundSensor.GetNormal();
    public Vector3 ContactPosition() =>  groundSensor.GetPosition();
    public Transform ContactTransform() =>  groundSensor.GetTransform();

    public bool enableSpring = true;

    public void AwakeGroundSpring()
    {
        tr = originTransform;
        rb = tr.GetComponent<Rigidbody>();
        col = tr.GetComponent<Collider>();
        ResetGroundSpring();
        
    }
    public void ResetGroundSpring()
    {
        rayStartPosition = col.bounds.center + originOffset;
    }
   
    void OnDrawGizmos()
    { 
       
        AwakeGroundSpring();
        SetupGroundSensor();
        #if  UNITY_EDITOR
        using (new Handles.DrawingScope(Color.red))
        {
            Handles.DrawLine(rayStartPosition, rayStartPosition + springLength * groundSensor.GetCastDirection(), 10f);
        }
        using (new Handles.DrawingScope(Color.green))
        {
            Handles.DrawLine(rayStartPosition, rayStartPosition + restDistance * groundSensor.GetCastDirection(), 5f);
        }
        SceneView.RepaintAll();
        #endif
    }

    public void CheckForGround()
    {
        ResetGroundSpring();
        SetupGroundSensor();
        if (useSphereCast)
        {
            groundSensor.Cast(sphereRadius);
        }
        else
        {
            groundSensor.Cast();
        }

        if (InContact())
        {
            ApplySpringForce();
            Debug.DrawLine(rayStartPosition, groundSensor.GetPosition(), Color.green);
        }
        else
        {
            Debug.DrawLine(rayStartPosition, rayStartPosition + springLength * groundSensor.GetCastDirection(), Color.red);
        }
        groundSensor.GetCastDirection();
    }
    public void ApplySpringForce()
    {
        if (!enableSpring) return;
        //get velocity in the spring direction
        float springVelocity = Vector3.Dot(rb.velocity, groundSensor.GetCastDirection());
        
        //get how much our spring has been offset from our rest distance
        float springOffset = groundSensor.GetDistance() - restDistance;
        
        //calculate the force from our strength and dampening
        float force = (springOffset * springStrength * 100f) - (springVelocity * springDampening * 10f);
        
        //add this force in the spring direction (the force itself will be negative if we need to go up)
        rb.AddForce(groundSensor.GetCastDirection() * force);
    }
    void SetupGroundSensor()
    {
        
        //null assigment operator, if we dont have a sensor, make one, else use the one that exists
        groundSensor ??= new RaycastSensor(tr,layerMask);


        if (extendSensor)
        {
            groundSensor.SetCastOrigin(rayStartPosition);
            groundSensor.SetCastDirection(springDirection);
            groundSensor.castLength = springLength * tr.localScale.x;
        }
        else
        {
            groundSensor.SetCastOrigin(rayStartPosition);
            groundSensor.SetCastDirection(springDirection);
            groundSensor.castLength = restDistance * tr.localScale.x;
        }
    }
}
