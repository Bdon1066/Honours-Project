using System;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]  
public class RobotMover : BaseMover
{
    [Header("Collider Settings")] 
    [Range(0f,1f)][SerializeField] float stepHeightRatio = 0.1f;
    [SerializeField] float colliderHeight = 2f;
    [SerializeField] float colliderThickness = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;
    
    Rigidbody rb;
    Transform tr;
    CapsuleCollider col;
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
        col = GetComponent<CapsuleCollider>();
        Disable();
        ReconfigureComponents();
        
    }
    public override void Enable()
    {
        col.enabled = true;
        rb.isKinematic = false;
        rb.detectCollisions = true;
        print("enable robot mover");
    }
    public override void Disable()
    {
        //col.enabled = false;
        //rb.isKinematic = true;
        //rb.detectCollisions = false;
        print("disbable robot mover");
    }

    //This function will reconfigure our rigidbody and collider to this mover's specifications
    void ReconfigureComponents()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        RecalculateColliderDimensions();
    }
    
    void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            RecalculateColliderDimensions();
        }
    }
    void LateUpdate() {
        if (debugMode) {
            sensor.DrawDebug();
        }
    }
    public void CheckForGround()
    {
        RecalibrateSensor();
        if (currentLayer != gameObject.layer)
        {
            RecalculateSensorLayerMask();
        }
        
        currentGroundAdjustmentVelocity = Vector3.zero;
        
        sensor.castLength =  usingExtendedSensorRange //extend sesnor range if required
            ? baseSensorRange + colliderHeight * tr.localScale.x * stepHeightRatio 
            : baseSensorRange;
        sensor.Cast();

        isGrounded = sensor.HasDetectedHit();
        if (!isGrounded) return;
        
        float distance = sensor.GetDistance();
        float upperLimit = colliderHeight * tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
        float middle = upperLimit + colliderHeight * tr.localScale.x * stepHeightRatio;
        float distanceToGo = middle - distance;
            
        currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
    }
    public bool IsGrounded() => isGrounded;
    public Vector3 GetGroundNormal() => sensor.GetNormal();
    
    public void SetVelocity(Vector3 velocity) => rb.velocity = velocity + currentGroundAdjustmentVelocity;
    public void SetExtendSensorRange(bool isExtended) => usingExtendedSensorRange = isExtended;
    void RecalculateColliderDimensions()
    {
        if (col == null) //i.e. in editor mode, need to run setup as Awake won't have been called
        {
            Init();
        }
        
        col.height = colliderHeight * (1f - stepHeightRatio);
        col.radius = colliderThickness / 2f;
        Vector3 stepHeightOffset = new Vector3(0f, stepHeightRatio * col.height / 2f, 0f); 
        col.center = colliderOffset * colliderHeight + stepHeightOffset;

        if (col.height / 2f < col.radius) //ensure radius is not too large and creating invalid capsule shape
        {
            col.radius = col.height / 2f;
        }
        
        RecalibrateSensor();
    }
   

    void RecalibrateSensor()
    {
        sensor ??= new RaycastSensor(tr); //ensures we have a sensor
        
        sensor.SetCastOrigin(col.bounds.center);
        sensor.SetCastDirection(RaycastSensor.CastDirection.Down);
        RecalculateSensorLayerMask();

        const float safetyDistanceFactor = 0.001f; //Mysterious and fun factor to prevent clipping

        float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
        baseSensorRange = length * (1f + safetyDistanceFactor) * tr.localScale.x;
        sensor.castLength = length * tr.localScale.x;
    }
    void RecalculateSensorLayerMask()
    {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;

        for (int i = 0; i < 32; i++) //iterate through all layers
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i)) //check if layer i ignores this mover's layer
            {
                layerMask &= ~(1 << i); //remove layer i from the mask via magical bitshifting
            }
        }

        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast"); // and do the same with ignore raycast layer
        layerMask &= ~(1 << ignoreRaycastLayer);

        sensor.layerMask = layerMask;
        currentLayer = objectLayer;
    }
}