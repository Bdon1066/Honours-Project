using System;
using UnityEngine;

public class RobotMode : BaseMode, IMovementStateController
{
    //MODE PROPERTIES
    Transform tr;
    Rigidbody rb;
    CapsuleCollider col;
    GroundSpring groundSpring;
    InputReader input;
    StateMachine stateMachine;
    
    [Header("Mode References")]
    [SerializeField] GameObject model;
    [SerializeField] Transform rootBone;
    
    [SerializeField] Transform cameraTransform;
    
    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnFall = delegate { };
    public event Action<Vector3> OnLand = delegate { };
    
    public event Action ToCar = delegate { };
    public event Action ToRobot = delegate { };
    
    
    //MOVEMENT PROPERTIES 
    
   
    [Header("Movement")]
    [Header("Ground")]
    public float maxSpeed = 10f;

    public float rotateSpeed = 10f;
    public float acceleration = 100f;
    public float maxAccelerationForce = 50f;
    public float slopeLimit = 45f;

    public float postTransformVelocityMultiplier = 2f;
    
    Vector3 currentVelocity;
    Vector3 velocityStep;
    [Header("Debug")]
    public bool debugMode;

    bool isEnabled;
    public bool isTransforming;
    public override Vector3 GetVelocity() => rb.velocity;
    public override Transform GetRootBone() => rootBone;
    public override void SetPosition(Vector3 position) => tr.transform.position = position; 
    
    void ShowModel() => model.SetActive(true);
    void HideModel() => model.SetActive(false);
    
    public override void AwakeMode(PlayerController playerController)
    {
        tr = transform;
        rb = tr.GetComponent<Rigidbody>();
        col = tr.GetComponent<CapsuleCollider>();
        input = playerController.input;
        
        groundSpring = tr.GetComponent<GroundSpring>();
        groundSpring.AwakeGroundSpring();

        SetupStateMachine();
        
        HideModel();
    }
    void SetupStateMachine() 
    {
        stateMachine = new StateMachine();
        var grounded = new GroundedState(this);
        //var jumping = new JumpingState(this);
        var falling = new FallingState(this);
        var rising = new RisingState(this);
        //var sliding = new SlidingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => !groundSpring.IsGrounded() && IsRising()));
        //At(grounded, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
        At(grounded, falling, new FuncPredicate(() => !groundSpring.IsGrounded()));
        //At(grounded, jumping, new FuncPredicate(() => (jumpIsPressed || jumpWasPressed) && !jumpInputLocked));

       // At(jumping, rising, new FuncPredicate(() => jumpTimer.IsFinished || jumpLetGo));
        
        At(falling, rising, new FuncPredicate(() => IsRising()));
        //At(falling, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
        At(falling, grounded, new FuncPredicate(() => groundSpring.IsGrounded() && !IsGroundTooSteep()));

        //At(rising, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
        At(rising, grounded, new FuncPredicate(() => groundSpring.IsGrounded() && !IsGroundTooSteep()));
        At(rising, falling, new FuncPredicate(() => IsFalling()));

        //At(sliding, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
        //At(sliding, falling, new FuncPredicate(() => !mover.IsGrounded()));
        //At(sliding, rising, new FuncPredicate(() => IsRising()));
        
        stateMachine.SetState(falling);
    }
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    bool IsRising() => Utils.GetDotProduct(rb.velocity, tr.up) > 0f;
    bool IsFalling() => Utils.GetDotProduct(rb.velocity, tr.up) < 0f;
    bool IsGroundTooSteep() => !groundSpring.IsGrounded() || Vector3.Angle(groundSpring.GroundNormal(), tr.up) > slopeLimit;
    public override void EnterMode(Vector3 entryVelocity)
    {
        // rb.AddForce(entryVelocity * 1000f);
        rb.velocity = entryVelocity * postTransformVelocityMultiplier;
        print(rb.velocity);
        isTransforming = false;
        ShowModel();
        isEnabled = true;
    }
    public override void TransformTo(BaseMode fromMode)
    {
        isTransforming = true;
        ShowModel();
        ToRobot.Invoke();
    }

    public override void TransformFrom(BaseMode toMode)
    {
        isTransforming = true;
        ToCar.Invoke();
    }
    public override void ExitMode()
    {
       HideModel();
       isEnabled = false;
    }

    void Update() => stateMachine.Update();
    void FixedUpdate()
    {
        //if (!isEnabled) return;
        HandleMovement();
        stateMachine.FixedUpdate();
    }
    void HandleMovement()
    {
        groundSpring.CheckForGround();
        
        SetRBRotation();
        HandleMoveInput();
    }
    void HandleMoveInput()
    {

        Vector3 moveDirection = GetMovementDirection();
        Vector3 maxMoveVelocity = moveDirection * maxSpeed;
        print(maxMoveVelocity);
        
        //set our velocity step to move towards our max velocity over acceleration
        velocityStep = Vector3.MoveTowards(velocityStep, maxMoveVelocity, acceleration * Time.fixedDeltaTime);
        
        //create an acceleration step from our velocity step
        Vector3 accelerationStep = (velocityStep - rb.velocity) / Time.fixedDeltaTime;
        
        //clamp our acceleratiopn to prevent abnormally high values
        accelerationStep = Vector3.ClampMagnitude(accelerationStep, maxAccelerationForce);
        
        //create a force, removing all force in the y direction
        Vector3 force = Vector3.Scale(accelerationStep * rb.mass,new Vector3(1,0,1)) ;
       
        rb.AddForce(force);
    }
    void SetRBRotation()
    {
        //if no velocity, don't rotate
        if (rb.velocity.magnitude < 0.001f) return;

        //get our velocity direction, ignoring Y
        Vector3 lookDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        //create a target rotation in that direction
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        //lerp toward that rotation via rotateSpeed
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
    }
    Vector3 GetMovementDirection()
    {
        //if we dont have camera, get input directly, else project camera onto our input to include it
        Vector3 direction = cameraTransform == null 
            ? tr.right * input.Direction.x + tr.forward * input.Direction.y 
            : Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.Direction.x + 
              Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.Direction.y;
        return direction;
    }

    public void OnJumpStart()
    {
        OnJump.Invoke(currentVelocity);
    }
    public void OnFallStart() 
    {
        OnFall.Invoke(currentVelocity);
    }
    
    public void OnGroundContactRegained()
    {
        OnLand.Invoke(currentVelocity);
    }

    
}