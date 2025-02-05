using UnityEngine;

public class CarMover : BaseMover
{
    [Header("Collider Settings")]
    [Range(0f, 1f)][SerializeField] float stepHeightRatio = 0.1f;
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


    public override void SetEnabled(bool value)
    {
        base.SetEnabled(value);
        if (IsEnabled()) ReconfigureComponents();
    }
    public override void Init()
    {
        base.Init();
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        col = GetComponent<CapsuleCollider>();

        ReconfigureComponents();

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
        col.direction = 2;
        RecalculateColliderDimensions();
    }
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

        //RecalibrateSensor();
    }
}

