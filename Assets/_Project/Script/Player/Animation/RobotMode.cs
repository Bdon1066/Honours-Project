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
    

    [SerializeField] GameObject model;
    [SerializeField] Transform rootBone;
    
    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnFall = delegate { };
    public event Action<Vector3> OnLand = delegate { };
    
    public event Action ToCar = delegate { };
    public event Action ToRobot = delegate { };

    public bool debugMode;
    
    //MOVEMENT PROPERTIES 
    
    //the velocity this frame
    Vector3 currentVelocity;

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

        
        HideModel();
    }
    public override void EnterMode(Vector3 entryVelocity)
    {
       rb.velocity = entryVelocity;
       
       ShowModel();
    }
    public override void TransformTo(BaseMode fromMode)
    {
        ShowModel();
        ToRobot.Invoke();
    }

    public override void TransformFrom(BaseMode toMode)
    {
        ToCar.Invoke();
    }
    public override void ExitMode()
    {
       HideModel();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }
    void HandleMovement()
    {
        groundSpring.CheckForGround();
        
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