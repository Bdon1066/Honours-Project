using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]  
public class CarMover : BaseMover
{
    [Header("Collider Settings")]
    [SerializeField] float colliderRadius= 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;

    Rigidbody rb;
    Transform tr;
    SphereCollider col;
    RaycastSensor sensor;

    bool isGrounded;
    float baseSensorRange;
    Vector3 currentGroundAdjustmentVelocity; //The velocity needed to adjust the player pos to correct ground pos over one frame
    int currentLayer;

    [Header("Sensor Settings:")]
    [SerializeField] bool debugMode;

    bool usingExtendedSensorRange = true; //For use pn uneven terrain



    public override void Init()
    {
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        col = GetComponent<SphereCollider>();
        Disable();
        ReconfigureComponents();

    }
    public override void Enable()
    {
        col.enabled = true;
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }
    public override void Disable()
    {
        col.enabled = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }
    void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            RecalculateColliderDimensions();
        }
    }

    //This function will reconfigure our rigidbody and collider to this mover's specifications
    void ReconfigureComponents()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;
        
        RecalculateColliderDimensions();
    }
    void RecalculateColliderDimensions()
    {
        if (col == null) //i.e. in editor mode, need to run setup as Init won't have been called
        {
            Init();
        }
       
        col.radius = colliderRadius;
        col.center = colliderOffset;
        
        //RecalibrateSensor();
    }
}

