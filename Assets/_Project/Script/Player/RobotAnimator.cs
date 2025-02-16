using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAnimator : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    RobotMode robot;
    Animator animator;

    readonly int speedHash = Animator.StringToHash("Speed");
    readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    readonly int isFallingHash = Animator.StringToHash("IsFalling");
    
    readonly int transformHash = Animator.StringToHash("Transform");

    void Start() {
        robot = GetComponent<RobotMode>();
        animator = GetComponentInChildren<Animator>();
            
        robot.OnJump += HandleJump;
        robot.OnFall += HandleFall;
        robot.OnLand += HandleLand;
        playerController.OnTransform += HandleTransform;
    }
   

    void Update() {
        animator.SetFloat(speedHash, robot.GetInputVelocityLastFrame().magnitude,0.1f,Time.deltaTime);
    }
    void HandleTransform(Vector3 momentum)
    {
        animator.SetTrigger(transformHash);
        print("Transform Go!");
        //animator.SetLayerWeight(1,1f);
    }
    void HandleJump(Vector3 momentum)
    {
        animator.SetBool(isJumpingHash, true);
        print("Jump");
    }
    void HandleFall(Vector3 momentum)
    {
        animator.SetBool(isJumpingHash, false);
        animator.SetBool(isFallingHash, true);
        print("Fall");
    }
    void HandleLand(Vector3 momentum)
    {
        animator.SetBool(isFallingHash, false);
        print("Land");
    }
}
