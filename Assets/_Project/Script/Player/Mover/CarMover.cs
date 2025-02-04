using UnityEngine;

public class CarMover : BaseMover
{
    [Header("Car Collider Settings")] 
    [SerializeField] float colliderHeight = 2f;
    [SerializeField] float colliderThickness = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;
    
    
    protected override void Awake()
    {
        base.Awake();
        RecalculateColliderDimensions();
    }
    public void EnterMover()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;
        col.direction = 2;
        RecalculateColliderDimensions();
    }
    
   
    void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            RecalculateColliderDimensions();
        }
    }

    

    public override void CheckForGround()
    {
        if (currentLayer != gameObject.layer)
        {
            RecalculateSensorLayerMask();
        }
        
        currentGroundAdjustmentVelocity = Vector3.zero;
        
        sensor.castLength =  usingExtendedSensorRange //extend sesnor range if required
            ? baseSensorRange + colliderHeight * tr.localScale.x
            : baseSensorRange;
        sensor.Cast();

        isGrounded = sensor.HasDetectedHit();
        if (!isGrounded) return;

        float distance = sensor.GetDistance();
        float upperLimit = colliderHeight * tr.localScale.x * 0.5f; //half the collider height above step area, top boundary of ideal pos
        float middle = upperLimit + colliderHeight * tr.localScale.x  ; // where the feet should be relative to ground
        float distanceToGo = middle - distance;
        
        currentGroundAdjustmentVelocity = tr.up *(distanceToGo / Time.fixedDeltaTime);
    }
    
    void RecalculateColliderDimensions()
    {
        if (col == null) //i.e. in editor mode, need to run setup as Awake won't have been called
        {
            Awake();
        }
        
        col.height = colliderHeight;
        col.radius = colliderThickness / 2f;
        col.center = colliderOffset * colliderHeight ;

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

        float length = colliderHeight * 0.5f + colliderThickness / 2f ; //length of our ray, includes adjust collider height and step height ratio
        baseSensorRange = length * (1f- safetyDistanceFactor) * tr.localScale.x;
        sensor.castLength = length * tr.localScale.x;
    }
    
    void RecalculateSensorLayerMask()
    {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;

        for (int i = 0; i < 32; i++) //iterate through all layers
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i)) //if object's layer ought to ignore layer i
            {
                layerMask &= ~(1 << i); //remove layer i from the mask via magical bitshifting
            }
        }

        int ignoreRaycastLayer = LayerMask.GetMask("Ignore Raycast"); // and do the same with ignore raycast layer
        layerMask &= ~(1 << ignoreRaycastLayer);

        sensor.layerMask = layerMask;
        currentLayer = objectLayer;
    }
    
}