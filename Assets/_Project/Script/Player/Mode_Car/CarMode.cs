using System;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;
public class CarMode : BaseMode, IMovementStateController
{

    #region Fields

    Transform tr;
    Rigidbody rb;
    BoxCollider col;
    InputReader input;
    StateMachine stateMachine;

    [SerializeField] GameObject model;
    [SerializeField] Transform rootBone;
    public Transform cameraTransform;

    Transform fromModeTr;

    [Header("Car Attributes")]
    public Axle[] axles = new Axle[2];
    [Header("Acceleration")]
    public float acceleration = 10000f;
    public float reverseAcceleration = 9000f;
    //This curve is how powerful the engine torque will be given our current speed
    public CurveScriptableObject accelerationPowerCurve;
    public CurveScriptableObject deccelerationPowerCurve;
    public float maxSpeed = 100f;
    [Header("Braking")]
    //The proportional breaking force applied when we are providing no input
    [Range(0, 1)] public float idleBrakeForce = 0.1f;
    [Header("Steering")]
    public float wheelTurnSpeed = 10f;
    public float maxSteerAngle = 45f;
    //for use in calculting wheel grip
    public float wheelMass = 10f;
    public float wheelGrip = 50f;
    [Range(0, 1)] public float frontWheelGrip = 0.5f;
    [Range(0, 1)] public float backWheelGrip = 0.4f;
    //public CurveScriptableObject frontWheelGripCurve;
    //public CurveScriptableObject backWheelGripCurve;

    [Header("Suspension")]
    //This is the distance our spring will want to rest at.
    public float restDistance = 1f;
    //How the maximum distance our spring can be offset from rest.
    public float maxSpringOffset = 0.5f;
    //How strong our spring is, stronger springs will result in faster osciliations.
    public float springStrength = 100f;
    //How strong our damping is, higher damping zeta will bring the spring to rest faster.
    [UnityEngine.Range(0.2f, 1f)] public float dampingZeta;
    public float wheelRadius = 0.33f;
    float dampingStrength;

    [Header("Debuig Settings")]
    public bool debugMode;
    public bool useAccelerationButton;
    bool isEnabled;

    bool isTransforming;
    bool isBraking;
    bool isAccelerating;

    #endregion

    public override Vector3 GetVelocity() => rb.velocity;
    public override Vector3 GetDirection()
    {
        return rb.velocity.normalized;
    }
    public override Transform GetRootBone() => rootBone;
    public override void SetPosition(Vector3 position) => tr.transform.position = position;

    void ShowModel() => model.SetActive(true);
    void HideModel() => model.SetActive(false);


    public override void Init(PlayerController playerController)
    {
        tr = transform;
        input = playerController.input;
        rb = GetComponent<Rigidbody>();
        col = GetComponentInChildren<BoxCollider>();

        CreateAxleWheels();
        SetupStateMachine();

        isEnabled = false;
        HideModel();

    }
    public override void EnterMode(Vector3 entryVelocity, Vector3 entryDirection)
    {
        isEnabled = true;
        isTransforming = false;

        rb.velocity = entryVelocity; //need to like add a force so that the super massive car can be at the same velocity as the robot when IN AIR
        entryDirection = new Vector3(rb.velocity.normalized.x, entryDirection.x, entryDirection.z);
        //rb.rotation = Quaternion.LookRotation(entryDirection, Vector3.up);
        ShowModel();

        input.Brake += HandleBrake;
        input.Accelerate += HandleAccelerate;
    }

    public override void TransformTo(BaseMode fromMode)
    {
        isTransforming = true;
        //cache our transfrom from modes transform so we can follow it
        fromModeTr = fromMode.GetRootBone();
    }

    public override void TransformFrom(BaseMode toMode)
    {
        HideModel();
    }

    public override void ExitMode()
    {
        input.Brake -= HandleBrake;
        input.Accelerate -= HandleAccelerate;
        HideModel();
        isEnabled = false;
    }

    private void HandleBrake(bool isButtonHeld)
    {
        if (isButtonHeld)
        {
            isBraking = true;
        }
        else
        {
            isBraking = false;
        }
        print("Braking: " + isBraking);

    }
    private void HandleAccelerate(bool isButtonHeld)
    {
        if (isButtonHeld)
        {
            isAccelerating = true;
        }
        else
        {
            isAccelerating = false;
        }
        print("Accelerating: " + isAccelerating);

    }

    void CreateAxleWheels()
    {
        for (int i = 0; i < axles.Length; i++)
        {
            axles[i].leftWheel = new WheelRay(axles[i].leftWheelTransform, this.gameObject);
            axles[i].rightWheel = new WheelRay(axles[i].rightWheelTransform, this.gameObject);
        }
    }
    void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var grounded = new GroundedState(this);
        var falling = new FallingState(this);
        var rising = new RisingState(this);

        //When at grounded state, we will go to rising state when IsRising is true
        At(grounded, rising, new FuncPredicate(() => !IsGrounded() && IsRising()));
        At(grounded, falling, new FuncPredicate(() => !IsGrounded() && IsFalling()));

        At(falling, rising, new FuncPredicate(() => IsRising()));
        At(falling, grounded, new FuncPredicate(() => IsGrounded()));

        At(rising, grounded, new FuncPredicate(() => IsGrounded()));
        At(rising, falling, new FuncPredicate(() => IsFalling()));


        stateMachine.SetState(falling);
    }
    public bool IsGrounded()
    {
        //if any of our wheels are grounded, set grounded true
        foreach (var axle in axles)
        {
            if (axle.leftWheel.IsGrounded() || axle.rightWheel.IsGrounded()) return true;
        }
        return false;
    }
    bool IsRising() => Utils.GetDotProduct(rb.velocity, tr.up) > 0f;
    bool IsFalling() => Utils.GetDotProduct(rb.velocity, tr.up) < 0f;
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    void Update()
    {
        stateMachine.Update();
    }
    void FixedUpdate()
    {
        if (isTransforming)
        {
            print("Transform Movement");
            HandleTransformingMovement();
        }
        else
        {
            stateMachine.Update();
            HandleMovement();
        }

    }

    private void HandleTransformingMovement()
    {
        //get our robot mode's transform and set position
        rb.position = fromModeTr.position;

        //get the rotation from robot
        Quaternion targetRotation = fromModeTr.rotation;

        //apply a 270 degree turn to the rotation so we start upright and fall into correct position
        targetRotation *= Quaternion.AngleAxis(270f, Vector3.right);
        rb.rotation = targetRotation;

    }

    void HandleMovement()
    {
        float accelerationInput = input.Direction.y;
        float steeringInput = input.Direction.x;

        //for each axle
        for (int i = 0; i < axles.Length; i++)
        {
            {
                //we want suspension  force on all wheels 
                HandleSuspension(axles[i].leftWheel);
                HandleSuspension(axles[i].rightWheel);

                //If disabled, we only want to handle suspesnion, so return
                if (!isEnabled) return;

                //send our steer input to the steering axle
                if (axles[i].steering)
                {
                    HandleSteeringInput(axles[i].leftWheel, steeringInput);
                    HandleSteeringInput(axles[i].rightWheel, steeringInput);
                }

                //If we arent grounded dont apply steering or acceleration forces
                if (stateMachine.CurrentState is not GroundedState) return;

                //we want steering force on all wheels,the lateral force on the wheels that works agaisnt sliding
                HandleSteering(axles[i].leftWheel, axles[i]);
                HandleSteering(axles[i].rightWheel, axles[i]);

                //send our acceleration input to our motor axle
                if (axles[i].motor)
                {
                    HandleAcceleration(axles[i].leftWheel, accelerationInput);
                    HandleAcceleration(axles[i].rightWheel, accelerationInput);
                }
            }

            //rb.rotation = Quaternion.LookRotation(rb.velocity);


            //rb.MoveRotation(Quaternion.LookRotation(rb.velocity));
            //slow to a stop if under teeny weeny velocities to stop idle sliding
            if (Mathf.Abs(rb.velocity.magnitude) < 0.01f)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.1f);
            }
        }
        void HandleSteeringInput(WheelRay wheelRay, float steeringInput)
        {
            float steerAngle = steeringInput * maxSteerAngle;
            Quaternion targetRotation = Quaternion.Euler(wheelRay.tr.localRotation.eulerAngles.x, steerAngle, wheelRay.tr.localRotation.eulerAngles.x);
            wheelRay.tr.localRotation = Quaternion.Slerp(wheelRay.tr.localRotation, targetRotation, Time.fixedDeltaTime * wheelTurnSpeed);
        }
        void HandleSteering(WheelRay wheelRay, Axle axle)
        {
            // float gripFactor = axleIndex > 0 ? backWheelGripCurve.Evaluate() : 0.0f;

            //Get the angle in the right vector
            //divide by 90 to get float , then clamp between 0 and 1 
            //feed that into the evaluate

            //Get our wheels current forward, steer direction and velocity
            Vector3 forwardDirection = wheelRay.tr.forward;
            Vector3 steeringDirection = wheelRay.tr.right;
            Vector3 wheelVelocity = rb.GetPointVelocity(wheelRay.tr.position);

            //calculate the magntude of our velocity in both forward and side direction
            float forwardMagnitude = Vector3.Project(wheelVelocity, forwardDirection).magnitude;
            float sideMagnitude = Vector3.Project(wheelVelocity, steeringDirection).magnitude;

            float steerVelocityRatio = Mathf.Clamp01(sideMagnitude * 5 / forwardMagnitude);


            float gripFactor = GetGripFactor(axle.axleLocation);
          
            //get our wheel's velocity in the steering direction
            float steeringVelocity = Vector3.Dot(steeringDirection, wheelVelocity);

            //apply an opposing grip velocity that opposes the steer force
            float velocityChange =   -steeringVelocity * gripFactor;

        //calculate acceleration from velocity
        float steerAcceleration = velocityChange / Time.fixedDeltaTime;

            //add this veolcity as a force taking into account wheel mass at each wheel position
            rb.AddForceAtPosition(steeringDirection * wheelMass * steerAcceleration, wheelRay.tr.position);

            if (debugMode) ////Show our direction of steerforce in yellow
            {
                Debug.DrawRay(wheelRay.tr.position, steeringDirection, Color.yellow);
            }
            if (debugMode) ////Show our direction of acceleration in blue
            {
                Debug.DrawRay(wheelRay.tr.position, forwardDirection, Color.blue);
            }
        }
        void HandleAcceleration(WheelRay wheelRay, float accelerationInput)
        {
            if (useAccelerationButton)
            {
                ButtonAcceleration(wheelRay, accelerationInput);
            }
            else 
            {
                StickAcceleration(wheelRay, accelerationInput);
            }

        }

        void HandleSuspension(WheelRay wheelRay)
        {
            dampingStrength = CalculateDampingStrength();

            float maxLength = restDistance + maxSpringOffset;
            //setup a suspesnion ray to start from the wheel force position, pointing downwards
            wheelRay.suspensionRay.SetCastOrigin(wheelRay.tr.position);
            wheelRay.suspensionRay.SetCastDirection(RaycastSensor.CastDirection.Down);
            wheelRay.suspensionRay.castLength = (maxLength + wheelRadius) * tr.localScale.x;

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
            float springOffset = (restDistance - (hitDistance - wheelRadius)) / maxSpringOffset;

            //Get the springs current velocity in the up direction
            float springVelocity = Vector3.Dot(wheelRay.tr.up, rb.GetPointVelocity(wheelRay.tr.position));

            //calculate force based on how far the spring is offset and the spring strength, and include damping
            float force = (springOffset * springStrength * 100f) - (springVelocity * dampingStrength); ;


            //apply upwards force to the rb at the wheelforce position
            rb.AddForceAtPosition(force * wheelRay.tr.up, wheelRay.tr.position);

            if (debugMode) //this red line shows our suspsension once we hit something
            {
                Debug.DrawLine(wheelRay.tr.position, hitPoint, Color.red);
            }
        }

        Vector3 CalculateWheelDirection(Transform wheelTransform)
        {

            Vector3 direction = cameraTransform == null //do we have a camera?
                ? wheelTransform.right * input.Direction.x //if not, direction determined by input directly
                : Vector3.ProjectOnPlane(cameraTransform.right, wheelTransform.up).normalized * input.Direction.x; //else direction determined by camera position plus input
                                                                                                                   //if direction greater than 1 normalize down to 1, else remain as is
            return direction.magnitude > 1f ? direction.normalized : direction;

        }
        //Use our damping zeta to calculate an ideal damping strength for our setup
        float CalculateDampingStrength()
        {
            return dampingZeta * (2 * Mathf.Sqrt(springStrength * rb.mass));
        }
    }

    private float GetGripFactor(Axle.AxleLocation axleLocation)
    {
        switch (axleLocation)
        {
            case Axle.AxleLocation.Front:
                return frontWheelGrip;
            case Axle.AxleLocation.Back:
                return backWheelGrip;
            default:
                return backWheelGrip;
        }
    }

    private void StickAcceleration(WheelRay wheelRay, float accelerationInput)
    {
        Vector3 accelerationDirection = wheelRay.tr.forward;

        //get our current forward speed
        float speed = Vector3.Dot(wheelRay.tr.forward, rb.velocity);

        //Get the ratio of our speed to our maxSpeed;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);

        //if at max speed, do not apply torque
        if (normalizedSpeed >= 1) { return; }

        //get our torque from the lookup curve and apply our accerlation based on input
        float torque = accelerationPowerCurve.Evaluate(normalizedSpeed) * accelerationInput * acceleration;

        //create force in the wheel forward direction from our torque
        rb.AddForceAtPosition(accelerationDirection * torque, wheelRay.tr.position);
        
    }

    private void ButtonAcceleration(WheelRay wheelRay, float accelerationInput)
    {
        Vector3 accelerationDirection = wheelRay.tr.forward;

        //get our current forward speed
        float speed = Vector3.Dot(wheelRay.tr.forward, rb.velocity);

        //Get the ratio of our speed to our maxSpeed;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);

        //if at max speed, do not apply torque
        if (normalizedSpeed >= 1) { return; }

        //get our torque from the lookup curve and apply our accerlation 
        float torque = accelerationPowerCurve.Evaluate(normalizedSpeed) * acceleration;

        //get our torque from the lookup curve and apply our reverse based on input so its most promimenent at the lowest input
        float reverseTorque = deccelerationPowerCurve.Evaluate(normalizedSpeed) * reverseAcceleration * accelerationInput;

        Vector3 acclerationForce = accelerationDirection * torque;
        Vector3 reverseForce = accelerationDirection * reverseTorque;

        if (isAccelerating && accelerationInput >= 0)
        {
            //create force in the wheel forward direction from our torque
            rb.AddForceAtPosition(acclerationForce, wheelRay.tr.position);
        }
        else if(isAccelerating && accelerationInput <= 0)
        {
            //create force in the wheel forward direction from our torque
            rb.AddForceAtPosition(reverseForce, wheelRay.tr.position);
        }
        else if (!isAccelerating && speed > 0) 
        {
            //create force in opposing direction scaled by some idle braking force
            Vector3 brakeForce = -acclerationForce * idleBrakeForce;
            rb.AddForceAtPosition(brakeForce, wheelRay.tr.position);
        }

        
    }
}


