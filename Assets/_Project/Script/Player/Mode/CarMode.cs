using System;
using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
    public WheelFrictionCurve originalForwardFriction;
    public WheelFrictionCurve originalSideFriction;
}
public class CarMode : BaseMode
{
    #region Fields
    
    //Rigidbody rb;
    CarMover mover;
    
    [Header("Axle Info")]
    public AxleInfo[] axleInfos;

    [Header("Motor Attributes")] 
    public float maxMotorTorque = 3000f;
    public float maxSpeed;

    [Header("Steering Attributes")]
    public float maxSteeringAngle = 30f;
    public AnimationCurve turnCurve;
    public float turnStrength = 1500f;
        
    [Header("Braking and Drifting")]
    public float driftSteerMultiplier = 1.5f; // Change in steering during a drift
    public float brakeTorque = 10000f;

    [Header("Physics")]
    public Transform centerOfMass;
    public float downForce = 100f;
    public float gravity = Physics.gravity.y;
    public float lateralGScale = 10f; // Scaling factor for lateral G forces;

    [Header("Banking")]
    public float maxBankAngle = 5f;
    public float bankSpeed = 2f;

    private float brakeVelocity;
    
    //OG Base Mode stuff below
    Vector3 savedVelocity, savedMovementVelocity;

    [Header("Camera Information")]
    [SerializeField] Transform cameraTransform;
    
    public event Action<Vector3> OnLand = delegate { };

    #endregion
    
    public Vector3 GetMovementVelocity() => savedMovementVelocity;

    protected override void Awake()
    { 
        base.Awake();
        //rb = GetComponent<Rigidbody>();
        mover = GetComponent<CarMover>();
        
        foreach (AxleInfo axleInfo in axleInfos) {
            axleInfo.originalForwardFriction = axleInfo.leftWheel.forwardFriction;
            axleInfo.originalSideFriction = axleInfo.leftWheel.sidewaysFriction;
        }
    }
    
    protected override void  SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var grounded = new GroundedState(this);
        var falling = new FallingState(this);
        var rising = new RisingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => IsRising()));
        At(grounded, falling, new FuncPredicate(() => !mover.IsGrounded()));
        
        
        At(falling, rising, new FuncPredicate(() => IsRising()));
        At(falling, grounded, new FuncPredicate(() => mover.IsGrounded()));
        
        At(rising, grounded, new FuncPredicate(() => mover.IsGrounded()));
        At(rising, falling, new FuncPredicate(() => IsFalling()));

        stateMachine.SetState(falling);
    }

    public override void Init(InputReader inputReader)
    {
        input = inputReader;
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        mover.CheckForGround();
        
        HandleMomentum();
        
        //if on the ground calculate velocity from player movement
        Vector3 velocity = stateMachine.CurrentState is GroundedState ? CalculateMovementVelocity() : Vector3.zero;
        velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
        
        UpdateAxles(velocity.x,velocity.y);

        //extend ground sensor range if on the ground
        mover.SetExtendSensorRange(IsGrounded());
        mover.SetVelocity(velocity);
        
        //save values for next frame
        savedVelocity = velocity;
        savedMovementVelocity = CalculateMovementVelocity();
        
    }

    public override void EnterMode(IState entryState, Vector3 entryMomentum)
    {
        stateMachine.SetState(entryState);
        momentum = entryMomentum;
        
        mover.EnterMover();
    }
    public override void EnterMode(Vector3 entryMomentum)
    {
        momentum = entryMomentum;
        
        mover.EnterMover();
    }

    public override void ExitMode()
    {
        mover.enabled = false;
        print("Disabled Car Mover");
    }


    void UpdateAxles(float motor, float steering) {
        foreach (AxleInfo axleInfo in axleInfos) {
            HandleSteering(axleInfo, steering);
            HandleMotor(axleInfo, motor);
            HandleBrakesAndDrift(axleInfo);
            //UpdateWheelVisuals(axleInfo.leftWheel);
            //UpdateWheelVisuals(axleInfo.rightWheel);
        }
    }
    void HandleSteering(AxleInfo axleInfo, float steering) {
        if (axleInfo.steering) {
            //float steeringMultiplier = input.IsBraking ? driftSteerMultiplier : 1f;
            axleInfo.leftWheel.steerAngle = steering; //* steeringMultiplier;
            axleInfo.rightWheel.steerAngle = steering; //* steeringMultiplier;
        }
    }
    void HandleMotor(AxleInfo axleInfo, float motor) {
        if (axleInfo.motor) {
            axleInfo.leftWheel.motorTorque = motor;
            axleInfo.rightWheel.motorTorque = motor;
        }
    }
    float AdjustInput(float input) {
        return input switch {
            >= .7f => 1f,
            <= -.7f => -1f,
            _ => input
        };
    }
    void HandleBrakesAndDrift(AxleInfo axleInfo) {
        if (axleInfo.motor) {
            if (input.IsBraking) {
                mover.ConstrainRigidBody(RigidbodyConstraints.FreezeRotationX);
                float newZ = Mathf.SmoothDamp(momentum.z, 0, ref brakeVelocity, 1f);
                momentum.z = newZ;
                    
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
                //ApplyDriftFriction(axleInfo.leftWheel);
                //ApplyDriftFriction(axleInfo.rightWheel);
            } else {
                mover.ConstrainRigidBody(RigidbodyConstraints.None);
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
               //ResetDriftFriction(axleInfo.leftWheel);
               // ResetDriftFriction(axleInfo.rightWheel);
            }
        }
    }
   
    void HandleMomentum()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
        

    }
    public void OnGroundContactLost()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
            
        Vector3 velocity = GetMovementVelocity();
        //TODO: Write comments here 
        if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f) 
        {
            Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);
            float dot = Utils.GetDotProduct(projectedMomentum.normalized, velocity.normalized);
            
            if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) velocity = Vector3.zero;
            else if (dot > 0f) velocity -= projectedMomentum;
        }
        momentum += velocity;
            
        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

 
    public void OnGroundContactRegained()
    {
        //send off current momentum for vfx/sfx/animator tracking
        Vector3 collisionVelocity = useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
        OnLand.Invoke(collisionVelocity);
    }

    Vector3 CalculateMovementVelocity()
    {
        Vector3 velocity = CalculateMovementDirection();
        //for a car, x determines Torque(speed) and y determines steering, so we apply different multipliers for each here
        velocity.x *= maxMotorTorque;
        velocity.y *= maxSteeringAngle;
        return velocity;
    } 
    
    Vector3 CalculateMovementDirection()
    {
        Vector3 direction = cameraTransform == null //do we have a camera?
            ? tr.right * input.Direction.x + tr.forward * input.Direction.y //if not, direction determined by input directly
            : Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.Direction.x + //else direction determined by camera position plus input
              Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.Direction.y;
        //if direction greater than 1 normalize down to 1, else remain as is
        return direction.magnitude > 1f ? direction.normalized : direction;

    }
}