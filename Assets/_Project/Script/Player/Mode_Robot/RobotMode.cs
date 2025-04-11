using System;
using UnityEngine;
using UnityEditor;


public class RobotMode : BaseMode, IMovementStateController
{
    //MODE PROPERTIES
    Transform tr;
    Rigidbody rb;
    CapsuleCollider col;
   
    InputReader input;
    StateMachine stateMachine;
    
    [Header("References")]
    [SerializeField] GameObject model;
    [SerializeField] Transform rootBone;
    
    public GroundSpring groundSpring;
    public Transform climbCutOff;
    public GroundSpring wallSpring;
    
    [SerializeField] Transform cameraTransform;

    RaycastSensor climbCutoffSensor;


    Transform fromModeTr;

    //MOVEMENT PROPERTIES 
    
    [Header("Movement")]
    [Header("Ground")]
    public float maxSpeed = 10f;
    public float rotateSpeed = 10f;
    public float groundAcceleration = 100f;
    public float maxGroundAccelerationForce = 150f;
    public float slopeLimit = 45f;

    [Header("Jump")]
    public float maxJumpHeight = 2f;
    public float maxJumpTime = 1f;
    [Range(0.5f, 4)] public float postApexGravity = 1.5f;
    float postApexGravityMultiplier;
    float initalJumpSpeed;

    [Header("In Air")]
    public float gravity = 9.8f;
    [Header("Wall")]
    public float maxWallSpeed = 10f;
    [Range(0,1)] public float horizontalWallSpeedScale;
    public float wallAcceleration = 100f;
    public float maxWallAccelerationForce = 50f;
    public float climbCutOffSensorLength;
    public float postClimbBoost = 5f;
    [Header("Wall Jump")]
    public float maxWallJumpHeight = 1f;
    public float maxWallJumpTime = 0.5f;
    public float horizontalWallJumpSpeed = 10f;
    [Header("Transforming")]
    public float postTransformVelocityMultiplier = 2f;

    bool jumpInputLocked, jumpLetGo, jumpIsPressed,jumpWasPressed,jumpVelocityAdded,wallJumpVelocityAdded;

    Vector3 velocityThisFrame, verticalVelocityThisFrame, horizontalVelocityThisFrame, velocityStep;
    
    [Header("Debug")]
    public bool debugMode;
    bool isEnabled;
    public bool isTransforming;
    
    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnFall = delegate { };
    public event Action<Vector3> OnLand = delegate { };
    public event Action<Vector3> OnWall = delegate { };
    public event Action OnEndClimb = delegate { };
    
    public event Action ToCar = delegate { };
    public event Action ToRobot = delegate { };
    public override Vector3 GetVelocity() => rb.velocity;
    public Vector3 GetHorizontalVelocity() => horizontalVelocityThisFrame;
    public Vector3 GetVerticalVelocity() => verticalVelocityThisFrame;
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
        
        groundSpring.AwakeGroundSpring();
        wallSpring.AwakeGroundSpring();
       
        SetupStateMachine();
        SetupJumpParameters(maxJumpHeight,maxJumpTime);
        
        HideModel();
    }
    void SetupStateMachine() 
    {
        stateMachine = new StateMachine();
        stateMachine.OnStateChanged += HandleStateChange;
        var grounded = new GroundedState(this);
        var jumping = new JumpingState(this);
        var falling = new FallingState(this);
        var rising = new RisingState(this);
        var wall = new WallState(this);
        var climbEnd = new ClimbEndState(this);
        var wallJumping = new WallJumpingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => !groundSpring.InContact() && IsRising()));
        At(grounded, wall, new FuncPredicate(() => IsOnWall() && !NegativeYInput()));
        At(grounded, falling, new FuncPredicate(() => !groundSpring.InContact()));
        At(grounded, jumping, new FuncPredicate(() => (jumpIsPressed || jumpWasPressed)  && !jumpInputLocked));

        At(jumping, rising, new FuncPredicate(() => jumpVelocityAdded));
        At(wallJumping, rising, new FuncPredicate(() => wallJumpVelocityAdded));
        
        At(falling, rising, new FuncPredicate(() => IsRising()));
        At(falling, wall, new FuncPredicate(() => IsOnWall() && !groundSpring.InContact()));
        At(falling, climbEnd, new FuncPredicate(() => AtTopOfClimb() && !groundSpring.InContact()));
        At(falling, grounded, new FuncPredicate(() => groundSpring.InContact() && !IsGroundTooSteep()));

        At(rising, wall, new FuncPredicate(() => IsOnWall() && !groundSpring.InContact() && !wallJumpVelocityAdded));
        At(rising, climbEnd, new FuncPredicate(() => AtTopOfClimb() && !groundSpring.InContact()));
        At(rising, grounded, new FuncPredicate(() => groundSpring.InContact() && !IsGroundTooSteep()));
        At(rising, falling, new FuncPredicate(() => IsFalling()));

        At(wall, grounded, new FuncPredicate(() => groundSpring.InContact() && NegativeYInput()));
        At(wall, falling, new FuncPredicate(() => !wallSpring.InContact()));
        At(wall, climbEnd, new FuncPredicate(() => AtTopOfClimb() && !NegativeYInput()));
        
        At(climbEnd, wall, new FuncPredicate(() => IsOnWall()));
        At(climbEnd, falling, new FuncPredicate(() => !wallSpring.InContact()));
        
        At(wall, wallJumping, new FuncPredicate(() => (jumpIsPressed || jumpWasPressed)  && !jumpInputLocked));
        
        stateMachine.SetState(falling);
    }
    private void HandleStateChange(IState obj)
    {
        print(obj.GetType().Name);
    }
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition)             => stateMachine.AddAnyTransition(to, condition);
    bool IsRising()                                       => Utils.GetDotProduct(rb.velocity, tr.up) > 0f;
    bool IsFalling()                                      => Utils.GetDotProduct(rb.velocity, tr.up) < 0f;
    bool IsGroundTooSteep()                               => !groundSpring.InContact() || Vector3.Angle(groundSpring.ContactNormal(), tr.up) > slopeLimit;
    bool AtTopOfClimb() => !climbCutoffSensor.HasDetectedHit() && wallSpring.InContact();
    bool IsOnWall() => climbCutoffSensor.HasDetectedHit() && wallSpring.InContact();

    bool NegativeYInput() => input.Direction.y <= 0;
    
    public override void EnterMode(Vector3 entryVelocity)
    {
        //add our previous mode velocity to this rb
        rb.velocity = entryVelocity * postTransformVelocityMultiplier;

        ShowModel();

        isTransforming = false;
        isEnabled = true;

        input.Jump += HandleJumpInput;
    }
    public override void TransformTo(BaseMode fromMode)
    {
        isTransforming = true;
        groundSpring.enableSpring = true;
        wallSpring.enableSpring = true;
        ShowModel();
        ToRobot.Invoke();
        fromModeTr = fromMode.GetRootBone();
    }

    public override void TransformFrom(BaseMode toMode)
    {
        ToCar.Invoke();
    }
    public override void ExitMode()
    {
       HideModel();
       isEnabled = false;
       input.Jump -= HandleJumpInput;
    }
   
    void Update() => stateMachine.Update();
    void FixedUpdate()
    {
        if (isTransforming)
        {
            HandleTransformMovement();
        }
        else
        {
            HandleMovement();
        }
        //if (!isEnabled) return;
        stateMachine.FixedUpdate();
        
    }

    void HandleTransformMovement()
    {
        if (fromModeTr == null) return;
        
        //get the rotation from car
        Quaternion targetRotation = fromModeTr.rotation;
        rb.rotation = targetRotation;
    }

    void HandleMovement()
    {
        ResetVelocityThisFrame();

        groundSpring.CheckForGround();
        wallSpring.CheckForGround();
        CheckClimbCutoff();
        
        SetRBRotation();
        
        if (stateMachine.CurrentState is WallState)
        {
            HandleWallMovement();
        }
        else if (stateMachine.CurrentState is ClimbEndState)
        {
            HandleClimbEndMovement();
        }
        else
        {
            HandleHorizontalMovement();
        }
        
        HandleJumping();
        HandleWallJumping();
        HandleGravity();
        
        ApplyVelocity();

        ResetJumpKeys();
    }
    void HandleWallMovement()
    {
        Vector3 moveDirection = GetWallMoveDirection();
        Vector3 wallDirection = -wallSpring.ContactNormal();
        Vector3 maxMoveVelocity = Vector3.Scale(moveDirection*maxWallSpeed, new Vector3(horizontalWallSpeedScale, 1, horizontalWallSpeedScale));
        
        //create a force
        Vector3 force = CreateForce(maxMoveVelocity, wallAcceleration, maxWallAccelerationForce);
        
        //remove forces in direction facing the wall
        force = Utils.RemoveDotVector(force,wallDirection);
        
        //add movement force to our velocity this frame
        verticalVelocityThisFrame.y += force.y;
        horizontalVelocityThisFrame.x += force.x;
        horizontalVelocityThisFrame.z += force.z;

        //enable ground detection when our vertical move direction is negative
        groundSpring.enableSpring = (moveDirection.y < 0);
        
    }
    private void HandleClimbEndMovement()
    {
        //move forward and upwards to reach over the wall
        Vector3 moveDirection = tr.up + tr.forward;
        //move with a boost of speed
        Vector3 maxMoveVelocity = moveDirection*(maxWallSpeed*postClimbBoost);
        Vector3 wallDirection = -wallSpring.ContactNormal();
        
        Vector3 force = CreateForce(maxMoveVelocity, wallAcceleration, maxWallAccelerationForce);
        //remove forces in direction facing the wall
        force = Utils.RemoveDotVector(force,wallDirection);
        
        //add movement force to our velocity this frame
        verticalVelocityThisFrame.y += force.y;
        horizontalVelocityThisFrame.x += force.x;
        horizontalVelocityThisFrame.z += force.z;
        
    }
    private void CheckClimbCutoff()
    {
        climbCutoffSensor ??= new RaycastSensor(climbCutOff);

        climbCutoffSensor.SetCastOrigin(climbCutOff.transform.position);
        climbCutoffSensor.SetCastDirection(CastDirection.Forward);
        climbCutoffSensor.castLength = climbCutOffSensorLength;
        
        climbCutoffSensor.Cast(0.25f);

    }
    void ResetVelocityThisFrame()
    {
        velocityThisFrame = Vector3.zero;
        horizontalVelocityThisFrame = Vector3.zero;
        verticalVelocityThisFrame = Vector3.zero;
    }
    void HandleHorizontalMovement()
    {
        Vector3 moveDirection = GetMovementDirection();
        Vector3 maxMoveVelocity = moveDirection * maxSpeed;
        
        //create a force, removing all force in the y direction
        Vector3 force = CreateForce(maxMoveVelocity,groundAcceleration,maxGroundAccelerationForce,new Vector3(1,0,1));

        //add movement force to our velocity this frame
        horizontalVelocityThisFrame += force;
    }
    Vector3 CreateForce(Vector3 maxVelocity, float acceleration, float maxAcceleration, Vector3 forceScale)
    {
        velocityStep = Vector3.MoveTowards(velocityStep, maxVelocity, acceleration * Time.fixedDeltaTime);
        
        //create an acceleration step from our velocity step
        Vector3 accelerationStep = (velocityStep - rb.velocity) / Time.fixedDeltaTime;
        
        //clamp our acceleratiopn to prevent abnormally high values
        accelerationStep = Vector3.ClampMagnitude(accelerationStep, maxAcceleration);
        
        //create a force, removing all force in the y direction
        return Vector3.Scale(accelerationStep * rb.mass,forceScale);
    }
    Vector3 CreateForce(Vector3 maxVelocity, float acceleration, float maxAcceleration)
    {
        velocityStep = Vector3.MoveTowards(velocityStep, maxVelocity, acceleration * Time.fixedDeltaTime);
        
        //create an acceleration step from our velocity step
        Vector3 accelerationStep = (velocityStep - rb.velocity) / Time.fixedDeltaTime;
        
        //clamp our acceleratiopn to prevent abnormally high values
        accelerationStep = Vector3.ClampMagnitude(accelerationStep, maxAcceleration);
        
        //create a force, removing all force in the y direction
        return accelerationStep * rb.mass;
    }
    void SetRBRotation()
    {
        Quaternion targetRotation;
        if (stateMachine.CurrentState is ClimbEndState) return;
        
        if (stateMachine.CurrentState is WallState)
        {
            //rotate to face the wall we are climbing
            targetRotation = Quaternion.FromToRotation(tr.forward,-wallSpring.ContactNormal()) * transform.rotation;
        }
        else
        {
            Vector3 horizontalMovement = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            //if no velocity, don't rotate
            if (horizontalMovement.magnitude < 0.05f) return;
        
            //get our velocity direction, ignoring Y
            Vector3 lookDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        
            //create a target rotation in that direction
            targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            //lerp toward that rotation via rotateSpeed
        }
        
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
    }
    private void SetupJumpParameters(float jumpHeight,float jumpTime)
    {
        float timeToApex = jumpTime / 2;

        //calculate  force of gravity and speed needed to achieve our jumpHeight over maxJumpTime
        gravity = (2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initalJumpSpeed = (2 * jumpHeight) / timeToApex;
    }
    void HandleJumping()
    {
        if (stateMachine.CurrentState is not JumpingState) return;

        jumpInputLocked = true;
        groundSpring.enableSpring = false;

        SetupJumpParameters(maxJumpHeight, maxJumpTime);

        verticalVelocityThisFrame.y = (initalJumpSpeed/Time.fixedDeltaTime) * rb.mass;
        
        jumpVelocityAdded = true;
    }
    void HandleWallJumping()
    {
        if (stateMachine.CurrentState is not WallJumpingState) return;
        
        jumpInputLocked = true;
        wallSpring.enableSpring = false;

        SetupJumpParameters(maxWallJumpHeight, maxWallJumpTime);
        
        Vector3 moveDirection = wallSpring.ContactNormal();
        Vector3 horizontalWallJumpVelocity = moveDirection * horizontalWallJumpSpeed;
        
        verticalVelocityThisFrame.y = (initalJumpSpeed/Time.fixedDeltaTime) * rb.mass;
        //horizontalVelocityThisFrame = horizontalWallJumpVelocity * rb.mass;
        rb.AddForce(wallSpring.ContactNormal() * horizontalWallJumpSpeed * rb.mass * Time.fixedDeltaTime, ForceMode.Impulse);
        wallJumpVelocityAdded = true;
    }
    void HandleGravity()
    {
        if (stateMachine.CurrentState is WallState) return;
        verticalVelocityThisFrame.y -= gravity * rb.mass * postApexGravityMultiplier;
    }
    
    void ApplyVelocity()
    {
        //MAYBE REMOVE THIS, COULD CAUSE CRAZINESS
        if (horizontalVelocityThisFrame.magnitude < 5f)
        {
            horizontalVelocityThisFrame = Vector3.zero;
        }
        velocityThisFrame = horizontalVelocityThisFrame + verticalVelocityThisFrame;
      
        rb.AddForce(velocityThisFrame);
    }
    void HandleJumpInput(bool isButtonPressed)
    {
        //print("Jump Event!");
        if (!jumpIsPressed && isButtonPressed)
        {
            jumpWasPressed = true;
        }

        if (jumpIsPressed && !isButtonPressed)
        {
            jumpLetGo = true;
            jumpInputLocked = false;
        }

        jumpIsPressed = isButtonPressed;

    }
    void ResetJumpKeys()
    {
        jumpLetGo = false;
        jumpWasPressed = false;
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
    Vector3 GetWallMoveDirection()
    {
        Vector3 direction = tr.right*input.Direction.x + tr.up*input.Direction.y;
            
        return direction;
    }

    public void OnJumpStart()
    {
        jumpInputLocked = true;
        OnJump.Invoke(velocityThisFrame);
    }
    public void OnWallJumpStart()
    {
        jumpInputLocked = true;
        OnJump.Invoke(velocityThisFrame);
    }
    public void OnFallStart() 
    {
        jumpVelocityAdded = false;
        wallJumpVelocityAdded = false;
        groundSpring.enableSpring = true;
        postApexGravityMultiplier = postApexGravity;
        wallSpring.enableSpring = true;
        OnFall.Invoke(velocityThisFrame);

    }
    
    public void OnGroundContactRegained()
    {
        print("Landed");
        postApexGravityMultiplier = 1f;
        OnLand.Invoke(velocityThisFrame);
    }
    public void OnWallStart()
    {
       // verticalVelocityThisFrame.y = 0;
        rb.velocity = Vector3.zero;
        postApexGravityMultiplier = 1f;
        groundSpring.enableSpring = false;
        ResetVelocityThisFrame();
        OnWall.Invoke(velocityThisFrame);
        print("StartingWall!");
    }
    public void OnClimbEnd()
    {
        OnEndClimb.Invoke();
    }
}