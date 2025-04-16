using System;
using ImprovedTimers;
using UnityEngine;
using UnityEditor;

public enum LandForce{Light,Medium,Heavy}
public class RobotMode : BaseMode, IMovementStateController
{
    //MODE PROPERTIES
    Transform tr;
    Rigidbody rb;
    CapsuleCollider col;
   
    InputReader input;
    public StateMachine stateMachine { get; private set; }
    
    [Header("References")]
    [SerializeField] GameObject model;
    [SerializeField] Transform rootBone;
    
    public GroundSpring groundSpring;
    public Transform climbCutOff;
    public GroundSpring wallSpring;
    
    [SerializeField] Transform cameraTransform;

    RaycastSensor climbCutoffSensor;
    RaycastSensor jumpSensor;


    Transform fromModeTr;

    CountdownTimer heavyLandTimer;
    CountdownTimer postTransformTimer;
    CountdownTimer postWallJumpTimer, postJumpTimer;
    CountdownTimer jumpCoyoteTimer, jumpBufferTimer;

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
    //this is used to prevent jumping again some time after landing (specifically medium landings)
    public float postJumpInputLockTime = 0.2f;
    public float coyoteTime = 0.5f;
    public float jumpBufferTime = 0.5f;
    [Range(0.5f, 4)] public float postApexGravity = 1.5f;
    [Space]
    public float maxWallJumpHeight = 1f;
    public float maxWallJumpTime = 0.5f;
    public float horizontalWallJumpSpeed = 10f;
    //this is used to prevent horizontal input some time after wall jumping
    public float postWallJumpInputLockTime = 0.5f;
    float initalJumpSpeed;
    [Header("Landing")]
    public float heavyLandThreshold = 30f;
    public float heavyLandTime = 0.2f;
    public float mediumLandThreshold = 30f;
    [Header("In Air")]
    public float gravity = 9.8f;
    public float maxFallSpeed = 20f;
    [Header("Wall")]
    public float maxWallSpeed = 10f;
    [Range(0,1)] public float horizontalWallSpeedScale;
    public float wallAcceleration = 100f;
    public float maxWallAccelerationForce = 50f;
    public float climbCutOffSensorLength;
    public float postClimbBoost = 5f;
    [Header("Transforming")]
    public float postTransformVelocityMultiplier = 2f;
    public float postTransformTime = 0.5f;
    [Header("Debug")]
    public bool debugMode;
    [HideInInspector] public bool isTransforming;
    
    bool jumpIsPressed,isJumping,isWallJumping;

    Vector3 velocityThisFrame, verticalVelocityThisFrame, horizontalVelocityThisFrame, velocityStep;
    bool isEnabled;
   
    
    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnFall = delegate { };
    public event Action<LandForce> OnLand = delegate { };
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
    

    float GetPostApexGravityMultiplier()
    {
        //if we are falling, or we are rising and have stopped holding jump, start falling with higher gravity
        bool notHoldingJump = stateMachine.CurrentState is RisingState && !IsJumpPressed();
        
        return ((stateMachine.CurrentState is FallingState & jumpCoyoteTimer.IsFinished) || notHoldingJump) ?  postApexGravity : 1f;
    }
    //prevent horizonmtal input when we are falling and the post transform timer is still running
    bool PreventHorizontalInput() => (stateMachine.CurrentState is FallingState && !postTransformTimer.IsFinished) || !postWallJumpTimer.IsFinished;
    public override void AwakeMode(PlayerController playerController)
    {
        tr = transform;
        rb = tr.GetComponent<Rigidbody>();
        col = tr.GetComponent<CapsuleCollider>();
        input = playerController.input;

        heavyLandTimer = new CountdownTimer(heavyLandTime);
        //adding the transform anim time (0.75f) to the time, as it's called while transforming
        postTransformTimer = new CountdownTimer(postTransformTime + 0.75f);
        postJumpTimer = new CountdownTimer(postJumpInputLockTime);
        postWallJumpTimer = new CountdownTimer(postWallJumpInputLockTime);
        jumpCoyoteTimer = new CountdownTimer(coyoteTime);
        jumpBufferTimer = new CountdownTimer(jumpBufferTime);
        
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
        var heavyLanded = new HeavyLandedState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => !groundSpring.InContact() && IsRising()));
        At(grounded, wall, new FuncPredicate(() => IsOnWall() && !NegativeYInput()));
        At(grounded, falling, new FuncPredicate(() => !groundSpring.InContact()));
        At(grounded, jumping, new FuncPredicate(() =>IsJumpPressed()  && !isJumping));
        
        //coyote jump
        At(falling, jumping, new FuncPredicate(() => IsJumpPressed()  && !isJumping && !jumpCoyoteTimer.IsFinished) );

        At(jumping, rising, new FuncPredicate(() => isJumping));
        At(wallJumping, rising, new FuncPredicate(() => isWallJumping));
        
        At(falling, rising, new FuncPredicate(() => IsRising()));
        At(falling, wall, new FuncPredicate(() => IsOnWall() && !groundSpring.InContact()));
        //At(falling, climbEnd, new FuncPredicate(() => AtTopOfClimb() && !groundSpring.InContact()));
        At(falling, grounded, new FuncPredicate(() => groundSpring.InContact() && !IsGroundTooSteep()&& !IsHeavyLanding()));
        At(falling, heavyLanded, new FuncPredicate(() => groundSpring.InContact() && !IsGroundTooSteep() && IsHeavyLanding()));
        
        At(heavyLanded, grounded, new FuncPredicate(() => groundSpring.InContact() && !IsGroundTooSteep() && heavyLandTimer.IsFinished));
        
        At(rising, wall, new FuncPredicate(() => IsOnWall() && !groundSpring.InContact() && !isWallJumping));
        At(rising, climbEnd, new FuncPredicate(() => AtTopOfClimb() && !groundSpring.InContact()));
        At(rising, grounded, new FuncPredicate(() => groundSpring.InContact() && !IsGroundTooSteep()));
        At(rising, falling, new FuncPredicate(() => IsFalling()));

        At(wall, grounded, new FuncPredicate(() => groundSpring.InContact() && NegativeYInput()));
        At(wall, falling, new FuncPredicate(() => !wallSpring.InContact()));
        At(wall, climbEnd, new FuncPredicate(() => AtTopOfClimb() && !NegativeYInput()));
        
        At(climbEnd, wall, new FuncPredicate(() => IsOnWall()));
        At(climbEnd, falling, new FuncPredicate(() => !wallSpring.InContact() || NegativeYInput()));
        
        At(wall, wallJumping, new FuncPredicate(() =>  jumpIsPressed  && !isWallJumping));
        
        stateMachine.SetState(grounded);
    }
    private void HandleStateChange(IState obj)
    {
        //print(obj.GetType().Name);
    }
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition)             => stateMachine.AddAnyTransition(to, condition);
    bool IsRising()                                       => Utils.GetDotProduct(rb.velocity, tr.up) > 0f;
    bool IsFalling()                                      => Utils.GetDotProduct(rb.velocity, tr.up) < 0f;
    bool IsGroundTooSteep()                               => !groundSpring.InContact() || Vector3.Angle(groundSpring.ContactNormal(), tr.up) > slopeLimit;
    bool AtTopOfClimb() => !climbCutoffSensor.HasDetectedHit() && wallSpring.InContact();
    bool IsOnWall() => climbCutoffSensor.HasDetectedHit() && wallSpring.InContact();
    
    bool IsHeavyLanding() => rb.velocity.y <= -heavyLandThreshold;

    bool IsMediumLanding() => rb.velocity.y <= -mediumLandThreshold && !IsHeavyLanding();
    bool        NegativeYInput() => input.Direction.y <= 0;

    bool IsJumpPressed() => jumpIsPressed || !jumpBufferTimer.IsFinished;
    
    public override void EnterMode(Vector3 entryVelocity)
    {
        //add our previous mode velocity to this rb
        rb.velocity = entryVelocity * postTransformVelocityMultiplier;
        ShowModel();

        isTransforming = false;
        isEnabled = true;

        input.Jump += HandleJumpInput;
        ///AAAAAAAAAAAAAAAAAAAAAAAAA LOWER FRAME RATES MESS WITH THE JUMP I KNEW IT 
        //Application.targetFrameRate = 30;
    }
    public override void TransformTo(BaseMode fromMode)
    {
        isTransforming = true;
        groundSpring.enableSpring = true;
        wallSpring.enableSpring = true;
        
        ShowModel();
        ToRobot.Invoke();
        fromModeTr = fromMode.GetRootBone(); 
        postTransformTimer.Start();
        HandleJumpInput(false);
    }

    public override void TransformFrom(BaseMode toMode)
    {
        if (stateMachine.CurrentState is WallState)
        {
           HandleJumpInput(true);
           rb.position += new Vector3(0,2.5f,0);
        }
        if (stateMachine.CurrentState is ClimbEndState)
        {
            HandleJumpInput(true);
        }
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
        wallSpring.extendSensor = true;
        SetRBRotation();
        
        if (stateMachine.CurrentState is WallState)
        {
            HandleWallMovement();
        }
        else if (stateMachine.CurrentState is ClimbEndState)
        {
            HandleClimbEndMovement();
        }
        else if (stateMachine.CurrentState is HeavyLandedState)
        {
          //do nothing 
        }
        else 
        {
            HandleHorizontalMovement();
        }
        
        if (stateMachine.CurrentState is JumpingState)
        {
            HandleJumping();
        }
        if (stateMachine.CurrentState is WallJumpingState)
        {
            HandleWallJumping();
        }
        
        HandleGravity();
        ApplyVelocity();
        groundSpring.extendSensor = stateMachine.CurrentState is not (FallingState or RisingState or JumpingState);
        ResetJump();
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
        if (PreventHorizontalInput()) return;
        
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
        verticalVelocityThisFrame.y = 0;
        if (!postJumpTimer.IsFinished) return;
        isJumping = true;
        groundSpring.enableSpring = false;

        SetupJumpParameters(maxJumpHeight, maxJumpTime);

        verticalVelocityThisFrame.y = (initalJumpSpeed/Time.fixedDeltaTime) * rb.mass;
    }
    void HandleWallJumping()
    {
        isWallJumping = true;
        wallSpring.enableSpring = false;

        SetupJumpParameters(maxWallJumpHeight, maxWallJumpTime);
        
        Vector3 moveDirection = wallSpring.ContactNormal();
        Vector3 horizontalWallJumpVelocity = moveDirection * horizontalWallJumpSpeed;
        
        verticalVelocityThisFrame.y = (initalJumpSpeed/Time.fixedDeltaTime) * rb.mass;
        //horizontalVelocityThisFrame = horizontalWallJumpVelocity * rb.mass;
        rb.AddForce((horizontalWallJumpVelocity/Time.fixedDeltaTime) * rb.mass, ForceMode.Impulse);
        postWallJumpTimer.Start();
    }
    void HandleGravity()
    {
        if (stateMachine.CurrentState is WallState or ClimbEndState) return;
        verticalVelocityThisFrame.y -= gravity * rb.mass * GetPostApexGravityMultiplier();
        print(GetPostApexGravityMultiplier());
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
        if (stateMachine.CurrentState is FallingState && isButtonPressed)
        {
            jumpBufferTimer.Start();
        }
        else
        {   
            jumpIsPressed = isButtonPressed;
        }
     
    }
    void ResetJump()
    {
        if (!jumpIsPressed && isJumping && stateMachine.CurrentState is GroundedState)
        {
            isJumping = false;
        }
        if (!jumpIsPressed && isWallJumping && stateMachine.CurrentState is WallState)
        {
            isWallJumping = false;
        }
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
        OnJump.Invoke(velocityThisFrame);
        
    }
    public void OnWallJumpStart()
    {
        OnJump.Invoke(velocityThisFrame);
    }
    public void OnFallStart() 
    {
        groundSpring.enableSpring = true;
        wallSpring.enableSpring = true;
        if (!isJumping)
        {
            jumpCoyoteTimer.Start();
        }
        OnFall.Invoke(velocityThisFrame);
    }
    
    public void OnGroundContactRegained()
    {
        OnLand.Invoke(DetermineLandForce());
    }
    private LandForce DetermineLandForce()
    {
        if (IsHeavyLanding())
        {
            return LandForce.Heavy;
        }
        if (IsMediumLanding())
        {
            postJumpTimer.Start();
            return LandForce.Medium;
        }
        return LandForce.Light;
    }
    public void OnWallStart()
    {
        rb.velocity = Vector3.zero;
        groundSpring.enableSpring = false;
        ResetVelocityThisFrame();
        OnWall.Invoke(velocityThisFrame);
        print("StartingWall!");
    }
    public void OnClimbEnd()
    {
        OnEndClimb.Invoke();
    }
    public void OnHeavyLandStart()
    {
        heavyLandTimer.Start();
        rb.velocity = Vector3.zero;
    }
}