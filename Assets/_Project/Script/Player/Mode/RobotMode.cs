using System;
using ImprovedTimers;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class RobotMode : BaseMode
{
    #region Fields

    RobotMover mover;
    
    //Bool flags for dealing with jump input states
    bool jumpInputLocked, jumpWasPressed, jumpLetGo, jumpIsPressed;
    
    [Header("Ground Attributes")]
    public float movementSpeed = 7f;
    public float groundFriction = 100f;
    [Header("Jump Attributes")]
    public float jumpSpeed = 10f;
    public float jumpDuration = 0.2f;
    [Header("In-Air Attributes")]
    public float gravity = 30f;
    public float airControlRate = 2f;
    [Range(0,1)]public float airControlScalingFactor = 0.25f;
    public float airFriction = 0.5f;
    [Header("Slide Attributes")]
    public float slideGravity = 5f;
    public float slopeLimit = 30f;
    
    Vector3 savedVelocity, savedMovementVelocity;
    
    CountdownTimer jumpTimer;
    [Header("Camera Info")]
    [SerializeField] Transform cameraTransform;

    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnLand = delegate { };

    #endregion
    
    public Vector3 GetMovementVelocity() => savedMovementVelocity;


    public override void Init(InputReader inputReader)
    {
       input = inputReader;
       input.Jump += HandleKeyJumpInput;
       mover = GetComponent<RobotMover>();
    }
    protected override void Awake()
    { 
        base.Awake();
        jumpTimer = new CountdownTimer(jumpDuration);
    }


    protected override void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var grounded = new GroundedState(this);
        var jumping = new JumpingState(this);
        var falling = new FallingState(this);
        var rising = new RisingState(this);
        var sliding = new SlidingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => IsRising()));
        At(grounded, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
        At(grounded, falling, new FuncPredicate(() => !mover.IsGrounded()));
        At(grounded, jumping, new FuncPredicate(() => (jumpIsPressed || jumpWasPressed) && !jumpInputLocked));

        At(jumping, rising, new FuncPredicate(() => jumpTimer.IsFinished || jumpLetGo));

        At(falling, rising, new FuncPredicate(() => IsRising()));
        At(falling, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
        At(falling, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));

        At(rising, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
        At(rising, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
        At(rising, falling, new FuncPredicate(() => IsFalling()));

        At(sliding, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
        At(sliding, falling, new FuncPredicate(() => !mover.IsGrounded()));
        At(sliding, rising, new FuncPredicate(() => IsRising()));

        stateMachine.SetState(falling);

    }
    
    bool IsGroundTooSteep() => !mover.IsGrounded() || Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit;
    

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        mover.CheckForGround();
        HandleMomentum();
        
        //if on the ground calculate velocity from player movement
        Vector3 velocity = stateMachine.CurrentState is GroundedState ? CalculateMovementVelocity() : Vector3.zero;
        velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

        //extend ground sensor range if on the ground
        mover.SetExtendSensorRange(IsGrounded());
        mover.SetVelocity(velocity);
        
        //save values for next frame
        savedVelocity = velocity;
        savedMovementVelocity = CalculateMovementVelocity();

        ResetJumpKeys();
    }

    public override void EnterMode(IState entryState, Vector3 entryMomentum)
    {
        stateMachine.SetState(entryState);
        momentum = entryMomentum;
        
        mover.SetupMover();
       
    }
    public override void ExitMode()
    {
        mover.enabled = false;
        print("Disabled Car Mover");
    }

    void HandleMomentum()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        Vector3 verticalMomentum = Utils.ExtractDotVector(momentum, tr.up); //extract vertical momentum
        Vector3 horizontalMomentum = momentum - verticalMomentum; //thus leaving horizontal remaining

        verticalMomentum -= tr.up * (gravity * Time.deltaTime); //add gravity

        //if on ground and still applying downward force, stop applying that downward force
        if (stateMachine.CurrentState is GroundedState && Utils.GetDotProduct(verticalMomentum, tr.up) < 0)
        {
            verticalMomentum = Vector3.zero;
        }

        //In-Air Momentum
            if (!IsGrounded()) 
            {
                AdjustInAirHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
            }
            //Sliding Momentum
            if (stateMachine.CurrentState is SlidingState)
            {
                HandleSliding(ref horizontalMomentum);
            }
            
            float friction = stateMachine.CurrentState is GroundedState ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum,Vector3.zero, friction * Time.deltaTime);

            momentum = horizontalMomentum + verticalMomentum;

            if (stateMachine.CurrentState is JumpingState)
            {
                HandleJumping();
            }

            if (stateMachine.CurrentState is SlidingState)
            {
                momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal()); // project momentum onto ground normal plane, ensuring smooth sliding over minute bumps
        
                if (Utils.GetDotProduct(momentum, tr.up) > 0f) momentum = Utils.RemoveDotVector(momentum, tr.up); //remove any upwards momentum if it exists
        
                Vector3 slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal().normalized); //projecting downward plane 
                momentum += slideDirection * (slideGravity * Time.deltaTime);
            }

            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;


    }
    void HandleKeyJumpInput(bool isButtonPressed)
    {
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
    public void OnJumpStart()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        momentum += tr.up * jumpSpeed;
        jumpTimer.Start();
        jumpInputLocked = true;
        OnJump.Invoke(momentum);
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

    void HandleJumping()
    {
        momentum = Utils.RemoveDotVector(momentum, tr.up);
        momentum += tr.up * jumpSpeed;
    }

    private void HandleSliding(ref Vector3 horizontalMomentum)
    {
        Vector3 pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized; //the direction of the slopes descent
        Vector3 movementVelocity = CalculateMovementVelocity();
        movementVelocity = Utils.RemoveDotVector(movementVelocity, pointDownVector); //remove player inputted velocity in direction of slope descent to prevent players movement adding onto the sliding
        horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        
    }
    Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;


    Vector3 CalculateMovementDirection()
    {

        Vector3 direction = cameraTransform == null //do we have a camera?
            ? tr.right * input.Direction.x + tr.forward * input.Direction.y //if not, direction determined by input directly
            : Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.Direction.x + //else direction determined by camera position plus input
              Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.Direction.y;
        //if direction greater than 1 normalize down to 1, else remain as is
        return direction.magnitude > 1f ? direction.normalized : direction;

    }

    void AdjustInAirHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity)
    {
        //for when we are movin faster than movement speed 
        if (horizontalMomentum.magnitude > movementSpeed)
        {
            //does our velocity have any component in the current horizontal momentum direction?
            if (Utils.GetDotProduct(movementVelocity,horizontalMomentum) > 0)
            {
                //remove overlapping velocity from momentum
                movementVelocity = Utils.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
            }
            //scale down our air control to maintain existing higher momentum while also having some control
            horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * airControlScalingFactor);
        }
        else
        {
            //if within movement speed, just add without scaling and clamp inside the speed
            horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
            horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
        }
    }




   
}
