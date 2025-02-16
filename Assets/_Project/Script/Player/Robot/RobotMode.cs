using System;
using ImprovedTimers;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
[RequireComponent(typeof(RobotMover))]  
public class RobotMode : BaseMode, IMovementStateController
{
    #region Fields
    
    Transform tr;
    RobotMover mover;
    InputReader input;
    PlayerController controller;

    [SerializeField] private GameObject model;

    bool jumpInputLocked, jumpWasPressed, jumpLetGo, jumpIsPressed;

    [Header("Ground Attributes")]
    public float movementSpeed = 7f;
    public float groundFriction = 100f;
    [Header("In-Air Attributes")]
    public float gravity = 30f;
    public float airControlRate = 2f;
    [Range(0,1)]public float airControlScalingFactor = 0.25f;
    public float airFriction = 0.5f;
    [Header("Jump Attributes")]
    public float jumpSpeed = 10f;
    public float jumpDuration = 0.2f;
    [Header("Slide Attributes")]
    public float slideGravity = 5f;
    public float slopeLimit = 30f;
    
    [Header("Momentum Settings")]
    public bool useLocalMomentum;
    Vector3 momentum, velocityLastFrame, inputVelocityLastFrame;
    
    CountdownTimer jumpTimer;
    StateMachine stateMachine;
    
    [Header("Camera")]
    [SerializeField] Transform cameraTransform;

    private bool isEnabled;

    

    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnFall = delegate { };
    public event Action<Vector3> OnLand = delegate { };

    #endregion


    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<CarMode>() != null)
        {
            print("Touching up to the car");
        }
    }

    bool IsGrounded() => stateMachine.CurrentState is GroundedState or SlidingState;
    public override Vector3 GetVelocity() => mover.GetVelocity();
    public override Vector3 GetDirection() => mover.GetDirection();
    
    public Vector3 GetInputVelocityLastFrame() => inputVelocityLastFrame;
    public override void SetPosition(Vector3 position) => tr.position = position;

    public StateMachine GetStateMachine() => stateMachine;

    void ShowModel() => model.SetActive(true);
    void HideModel() => model.SetActive(false);
    void SetEnabled(bool value) => isEnabled = value;
    public  bool IsEnabled() => isEnabled;
    
    //Initialize Mode with our player controllers input, called in PlayerController Awake()
    public override void Init(PlayerController playerController)
    {  tr = transform; 
        controller = playerController;
       input = playerController.input;
       mover = GetComponent<RobotMover>();
       mover.Init();
       
       SetEnabled(false);
       HideModel();

       controller.OnTransform += HandleTransform;
       
       jumpTimer = new CountdownTimer(jumpDuration);
       SetupStateMachine();
    }
   
    public override void EnterMode(Vector3 entryVelocity, Vector3 entryDirection)
    {
        momentum = entryVelocity;
        
        OnEnter();
    }
    public override void HandleTransform(Vector3 momentum)
    {
        
    }
    void OnEnter()
    {
        SetEnabled(true);
        ShowModel();
        mover.Enable();
        
        input.Jump += HandleKeyJumpInput;

        //print("Entering Robot Mode");
    }
    public override void ExitMode() => OnExit();
    void OnExit()
    {
        //print("Exiting Robot Mode");
        SetEnabled(false);
        HideModel();
        input.Jump -= HandleKeyJumpInput;
        mover.Disable();
    }
    void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var grounded = new GroundedState(this);
        var jumping = new JumpingState(this);
        var falling = new FallingState(this);
        var rising = new RisingState(this);
        var sliding = new SlidingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => !mover.IsGrounded() && IsRising()));
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
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    bool IsRising() => Utils.GetDotProduct(GetVelocity(), tr.up) > 0f;
    bool IsFalling() => Utils.GetDotProduct(GetVelocity(), tr.up) < 0f;
    bool IsGroundTooSteep() => !mover.IsGrounded() || Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit;
    void Update()
    {
        //if this mode is disabled, return out of update
        if (!IsEnabled()) return;
        stateMachine.Update();
    }

  


    void FixedUpdate()
    {
        //if this mode is disabled, return out of update
        if (!IsEnabled()) return;
        stateMachine.FixedUpdate();
        
        mover.CheckForGround();
        HandleMomentum();
        
        //If grounded, calculate velocity from input
        Vector3 velocity = stateMachine.CurrentState is GroundedState ? CalculateMovementInputVelocity() : Vector3.zero;
        //add this frames input velocity to our current momentum
        velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

        //extend ground sensor range if on the ground
        mover.SetExtendSensorRange(IsGrounded());
        
        mover.SetVelocity(velocity);
        
        //save values for next frame
        velocityLastFrame = velocity;
        inputVelocityLastFrame = CalculateMovementInputVelocity();

        ResetJumpKeys();

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
            AdjustInAirHorizontalMomentum(ref horizontalMomentum, CalculateMovementInputVelocity());
        }
        //Sliding Momentum
        if (stateMachine.CurrentState is SlidingState)
        {
            HandleSliding(ref horizontalMomentum);
        }
        
        //Handle friction, which is a constant force slowing our horizontal momentum
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
    public void OnJumpStart()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        momentum += tr.up * jumpSpeed;
        jumpTimer.Start();
        jumpInputLocked = true;
        OnJump.Invoke(momentum);
        print("On Jump Start");
    }
    public void OnFallStart() {
        
        var currentUpMomemtum = Utils.ExtractDotVector(momentum, tr.up);
        momentum = Utils.RemoveDotVector(momentum, tr.up);
        momentum -= tr.up * currentUpMomemtum.magnitude;
        OnFall.Invoke(momentum);
    }
    public void OnGroundContactLost()
    { if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
            
        Vector3 velocity = GetInputVelocityLastFrame();
        if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f) {
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
        //remove all existing upwards momentum
        momentum = Utils.RemoveDotVector(momentum, tr.up);
        //add our upwards jump momentum
        momentum += tr.up * jumpSpeed;
    }

    private void HandleSliding(ref Vector3 horizontalMomentum)
    {
        Vector3 pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized; //the direction of the slopes descent
        Vector3 movementVelocity = CalculateMovementInputVelocity();
        movementVelocity = Utils.RemoveDotVector(movementVelocity, pointDownVector); //remove player inputted velocity in direction of slope descent to prevent players movement adding onto the sliding
        horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        
    }
    Vector3 CalculateMovementInputVelocity() => CalculateMovementDirection() * movementSpeed;


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