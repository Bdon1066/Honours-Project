using ImprovedTimers;
using UnityEngine;

[RequireComponent(typeof(CarMover))]
public class CarMode : BaseMode
{

    Transform tr;
    CarMover mover;
    InputReader input;
    PlayerController controller;
    
    public GameObject model;


    public float driveSpeed = 200f;
    public float gravity = 30f;
    
    [Header("Momentum Settings")]
    public bool useLocalMomentum;
    Vector3 momentum, savedVelocity, savedMovementVelocity;
    
    //StateMachine stateMachine;
    
    [Header("Camera")]
    [SerializeField] Transform cameraTransform;
    
    private bool isEnabled;
    
    void ShowModel() => gameObject.SetActive(true);
    void HideModel() => gameObject.SetActive(true);
    void SetEnabled(bool value) => isEnabled = value;
    public bool IsEnabled() => isEnabled;
    
    public override Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
    public override Vector3 GetMovementVelocity() => savedMovementVelocity;
    public override void SetPosition(Vector3 position) => mover.SetPosition(position);


    public override void Init(PlayerController playerController)
    {  tr = transform; 
        controller = playerController;
        input = playerController.input;
      
        mover = GetComponent<CarMover>();
        mover.Init();
        
        
        
       // SetupStateMachine();
    }
    public override void EnterMode(Vector3 entryMomentum)
    {
        momentum = entryMomentum;
        
        OnEnter();
    }
    public override void EnterMode() => OnEnter();
    void OnEnter()
    {
        SetEnabled(true);
        ShowModel();
       
        mover.Enable();
        
    }
    public override void ExitMode() => OnExit();
    void OnExit()
    {
        SetEnabled(false);
        HideModel();
        
        mover.Disable();
    }
    
    void FixedUpdate()
    {
        //if this mode is disabled, return out of update
        if (!IsEnabled()) return;
        //stateMachine.FixedUpdate();
        
        mover.CheckForGround();
        HandleMomentum();
        
        //If grounded, calculate fwd accerelartion
        //Vector3 acceleration = stateMachine.CurrentState is GroundedState ? CalculateForwardAcceleration() : Vector3.zero;
        Vector3 acceleration = CalculateForwardAcceleration();
        
        //add this frames input velocity to our current momentum
        acceleration  += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

        //extend ground sensor range if on the ground
       // mover.SetExtendSensorRange(IsGrounded());
        
        mover.SetAcceleraton(acceleration);
        
        //save values for next frame
        savedVelocity = acceleration;
        savedMovementVelocity = CalculateMovementInputVelocity();
        

    }
    void HandleMomentum()
    {
        Vector3 verticalMomentum = Utils.ExtractDotVector(momentum, tr.up); //extract vertical momentum
        Vector3 horizontalMomentum = momentum - verticalMomentum; //thus leaving horizontal remaining

        verticalMomentum -= tr.up * (gravity * Time.deltaTime); //add gravity
        
        momentum = horizontalMomentum + verticalMomentum;
        

        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    Vector3 CalculateForwardAcceleration() => tr.forward * input.Direction.y * driveSpeed;

    Vector3 CalculateMovementInputVelocity() => CalculateMovementDirection();// * movementSpeed;


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


