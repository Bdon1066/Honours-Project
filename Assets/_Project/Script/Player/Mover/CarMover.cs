using System;
using UnityEngine;

public class CarMover : BaseMover
{
    [Header("Sphere Motor Collider Settings")]
    [SerializeField] float colliderRadius = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;

    
    [SerializeField] CarSphereMotor sphereMotor;
    
    //[SerializeField] Transform car;

    Transform tr;
    RaycastSensor sensor;
   

    bool isGrounded;
    float baseSensorRange;
    Vector3 currentGroundAdjustmentVelocity; //The velocity needed to adjust the player pos to correct ground pos over one frame
    int currentLayer;

    [Header("Sensor Settings:")]
    [SerializeField] bool debugMode;

    bool usingExtendedSensorRange = true; //For use pn uneven terrain
    
    public bool IsGrounded() => isGrounded;
    public Vector3 GetGroundNormal() => sensor.GetNormal();
    
   // public void SetVelocity(Vector3 velocity) => rb.velocity = velocity + currentGroundAdjustmentVelocity;

    public void SetAcceleraton(Vector3 force)
    {
        sphereMotor.rb.AddForce(force + currentGroundAdjustmentVelocity, ForceMode.Acceleration);
    }
    public void SetExtendSensorRange(bool isExtended) => usingExtendedSensorRange = isExtended;
    //set the position of the sphere motor, this object will update to the new position
    public void SetPosition(Vector3 position)
    {
        sphereMotor.tr.position = position;
    }

    public override void Init()
    {
        tr = GetComponent<Transform>();
        Disable();
        ReconfigureComponents();
        
        //unparent the motor, as we want this to be sepearte from our main car object
        sphereMotor.tr.SetParent(null);

    }
    public override void Enable()
    {
        sphereMotor.col.enabled = true;
        sphereMotor.rb.isKinematic = false;
        sphereMotor.rb.detectCollisions = true;
    }
    public override void Disable()
    {
    }
    //void OnValidate()
    //{
    //    if (gameObject.activeInHierarchy)
    //    {
    //        RecalculateColliderDimensions();
    //    }
    //}
    void Update()
    {
       tr.position = sphereMotor.tr.position;
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
            ? baseSensorRange + colliderRadius * tr.localScale.x
            : baseSensorRange;
        sensor.Cast();

        isGrounded = sensor.HasDetectedHit();
        if (!isGrounded) return;
        print("Car is grounded");
        
        float distance = sensor.GetDistance();
        float upperLimit = colliderRadius * 2;
        float middle = upperLimit + colliderRadius * tr.localScale.x;
        float distanceToGo = middle - distance;
            
        currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
    }

    //This function will reconfigure our rigidbody and collider to this mover's specifications
    void ReconfigureComponents()
    {
        sphereMotor.rb.useGravity = false;
        sphereMotor.rb.freezeRotation = true;
        
        RecalculateColliderDimensions();
    }
    void RecalculateColliderDimensions()
    {
        if (sphereMotor.col == null) //i.e. in editor mode, need to run setup as Init won't have been called
        {
            Init();
        }
       
        sphereMotor.col.radius = colliderRadius;
        sphereMotor.col.center = colliderOffset;
        
        RecalibrateSensor();
    }
    
    void RecalibrateSensor()
    {
        sensor ??= new RaycastSensor(tr); //ensures we have a sensor
        
        sensor.SetCastOrigin(sphereMotor.col.bounds.center);
        sensor.SetCastDirection(RaycastSensor.CastDirection.Down);
        RecalculateSensorLayerMask();

        const float safetyDistanceFactor = 0.001f; //Mysterious and fun factor to prevent clipping

        float length = colliderRadius;
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

