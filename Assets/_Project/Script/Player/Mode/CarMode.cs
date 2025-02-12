using System;
using System.Diagnostics;
using ImprovedTimers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;


public class CarMode : BaseMode
{

    #region Fields

    Transform tr;
    Rigidbody rb;
    BoxCollider col;
    InputReader input;
    StateMachine stateMachine;
    
    
    [SerializeField] Transform[] wheelRayTransforms;
    WheelRay[] wheelRays = new WheelRay[4];

    [Header("Car Attributes")] 
    public float acceleration = 25f;
    //This curve is how powerful the engine torque will be given our current speed
    public AnimationCurve powerCurve;
    //public float deceleration = 25f;
    public float maxSpeed = 100f;
    
    [Header("Suspension")] 
    //This is the distance our spring will want to rest at.
    public float restDistance = 1f;
    //How the maximum distance our spring can be offset from rest.
    public float maxSpringOffset = 0.5f;
    //How strong our spring is, stronger springs will result in faster osciliations.
    public float springStrength = 100f;
    //How strong our damping is, higher damping zeta will bring the spring to rest faster.
    [Range(0.2f,1f)]public float dampingZeta;
    public float wheelRadius = 0.33f;
    float dampingStrength ;
    
    
    Vector3 currentVelocity;
    //this reprsents how fast we are going compared to maxSpeed, for use in accerlating the car
    float velocityRatio;
    
    public bool debugMode;
    
    
    #endregion
    
    public override Vector3 GetMomentum() => Vector3.zero;
    public override Vector3 GetMovementVelocity() => Vector3.zero;
    public override void SetPosition(Vector3 position) => tr.transform.position = position;
    

    public override void Init(PlayerController playerController)
    {
        tr = transform;
        input = playerController.input;
        rb = GetComponent<Rigidbody>();
        col = GetComponentInChildren<BoxCollider>();

        CreateWheelForces();
        ReadjustCOM();
        SetupStateMachine();
        
    }
    void CreateWheelForces()
    {
        for (int i = 0; i < wheelRayTransforms.Length; i++)
        {
            wheelRays[i] = new WheelRay(wheelRayTransforms[i]);
            //Recaculate wheel force rays to include this game objects layer collision stuff
            wheelRays[i].RecalculateSensorLayerMask(ref wheelRays[i].suspensionRay,this.gameObject);
            
        }
    }
    void ReadjustCOM()
    {
        float totalZValue = 0;
        
        for (int i = 0; i < wheelRays.Length; i++)
        {
            totalZValue += wheelRays[i].tr.localPosition.z;
        }
        float newZ = totalZValue / wheelRays.Length;
        //rb.centerOfMass = new Vector3(0, 0, newZ);
    }
    void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var grounded = new GroundedState(this);
        //var falling = new FallingState(this);
        //var rising = new RisingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        //At(grounded, rising, new FuncPredicate(() => IsRising()));
        //At(grounded, falling, new FuncPredicate(() => IsGrounded()));
        
        //At(falling, rising, new FuncPredicate(() => IsRising()));
        //At(falling, grounded, new FuncPredicate(() => IsGrounded()));
        
        //At(rising, grounded, new FuncPredicate(() => IsGrounded()));
        //At(rising, falling, new FuncPredicate(() => IsFalling()));
        

        //stateMachine.SetState(grounded);
    }
    public bool IsGrounded()
    {
        //if any of our wheels are grounded, the whole car is grounded
        foreach (WheelRay wheelRay in wheelRays)
        {
            if (wheelRay.IsGrounded()) return true;
        }
        return false;
    }
    bool IsRising() => Utils.GetDotProduct(GetMomentum(), tr.up) > 0f;
    bool IsFalling() => Utils.GetDotProduct(GetMomentum(), tr.up) < 0f;
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    
    public override void EnterMode(Vector3 entryMomentum)
    {
        //noop
    }
    public override void EnterMode()
    {
       //noop
    }
    public override void ExitMode()
    {
        //noop
    }
    void FixedUpdate()
    {
        HandleMovement();
    }
    void HandleMovement()
    {
        float accelerationInput = input.Direction.y;
        float steeringInput = input.Direction.x;

        foreach (var wheelRay in wheelRays)
        {
            HandleSuspension(wheelRay);
            HandleAcceleration(wheelRay, accelerationInput * acceleration);
        }
      

       //slow to a stop if under teeny weeny velocities to stop idle sliding
        if (Mathf.Abs(rb.velocity.magnitude) < 0.01f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.1f);
        }
    }
    void HandleAcceleration(WheelRay wheelRay, float accelerationInput)
    {
        Vector3 accelerationDirection = wheelRay.tr.forward;
        
        //get our current forward speed
        float speed = Vector3.Dot( wheelRay.tr.forward,rb.velocity);
        
        //Get the ratio of our speed to our maxSpeed;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);

        float torque = powerCurve.Evaluate(normalizedSpeed) * accelerationInput;
        
        print(torque);
        
       rb.AddForceAtPosition(accelerationDirection * torque, wheelRay.tr.position);

    }

    void HandleSuspension(WheelRay wheelRay)
    {
        dampingStrength = CalculateDampingStrength();
        
        float maxLength = restDistance + maxSpringOffset;
        //setup a suspesnion ray to start from the wheel force position, pointing downwards
        wheelRay.suspensionRay.SetCastOrigin(wheelRay.tr.position);
        wheelRay.suspensionRay.SetCastDirection(RaycastSensor.CastDirection.Down);
        wheelRay.suspensionRay.castLength =  (maxLength + wheelRadius) * tr.localScale.x;
        
        wheelRay.suspensionRay.Cast();
        
        //if we havent hit anything, bail out
        if (!wheelRay.suspensionRay.HasDetectedHit())
        {
            if (debugMode) //this green line shows our raycast
            {
                Debug.DrawLine(wheelRay.tr.position,
                    wheelRay.tr.position + (wheelRadius + maxLength) * -wheelRay.tr.up, Color.green);
            }
            return;
        }
 
        Vector3 hitPoint = wheelRay.suspensionRay.GetPosition();
        float hitDistance = wheelRay.suspensionRay.GetDistance();
        
        //calculate how much our spring is offset from rest
        float springOffset = (restDistance - (hitDistance - wheelRadius)) /maxSpringOffset;
        
        //Get the springs current velocity in the up direction
        float springVelocity = Vector3.Dot(wheelRay.tr.up, rb.GetPointVelocity(wheelRay.tr.position));
        
        //calculate force based on how far the spring is offset and the spring strength, and include damping
        float force = (springOffset * springStrength * 100f) -  (springVelocity * dampingStrength);;
        
        
        //apply upwards force to the rb at the wheelforce position
        rb.AddForceAtPosition(force * wheelRay.tr.up,wheelRay.tr.position);
        
        if (debugMode) //this red line shows our suspsension once we hit something
        {
            Debug.DrawLine(wheelRay.tr.position, hitPoint, Color.red);
        }
    } 
    //Use our damping zeta to calculate an ideal damping strength for our setup
    float CalculateDampingStrength()
    {
      return dampingZeta * (2 * Mathf.Sqrt(springStrength * rb.mass));
    }

    
  


}


