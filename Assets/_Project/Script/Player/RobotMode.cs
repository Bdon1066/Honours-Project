using System;
using UnityEngine;

public class RobotMode : BaseMode
{
    #region Fields

    [SerializeField] private InputReader input;

    Transform tr;
    IMover mover;

    bool jumpInputLocked, jumpWasPressed, jumpLetGo, jumpIsPressed;

    public float movementSpeed = 7f;
    public float groundFriction = 100f;
    public float gravity = 30f;
    
    public float jumpSpeed = 10f;
    public float jumpDuration = 0.2f;
    public float airControlRate = 2f;
    public float airFriction = 0.5f;
   
    public float slideGravity = 5f;
    
    public bool useLocalMomentum;
    
    //StateMachine stateMachine;
    //CountdownTimer jumpTimer;

    [SerializeField] Transform cameraTransform;

    Vector3 momentum, savedVelocity, savedMovementVelocity;

    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnLand = delegate { };

    #endregion

    void Awake()
    {
        tr = transform;
        mover = GetComponent<IMover>();
        
        //jumpTimer = new CountdownTimer(jumpDuration)
        StateMachine stateMachine;
    }

    public Vector3 GetMomentum => useLocalMomentum
        ? tr.localToWorldMatrix * momentum
        : momentum;

    void FixedUpdate()
    {
        mover.CheckForGround();
        HandleMomentum();
    }
    void HandleMomentum()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        Vector3 verticalMomentum = Utils.ExtractDotVector(momentum, tr.up); //extract vertical momentum
        Vector3 horizontalMomentum = momentum - verticalMomentum; //thus leaving horizontal remaining

        verticalMomentum -= tr.up * (gravity * Time.deltaTime); //add gravity
        
    }
    
    
}
