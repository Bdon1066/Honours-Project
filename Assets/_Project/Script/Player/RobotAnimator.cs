using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This struct is used for savinbg each bones transform before and after using the wacky Transforming animations that break everything
/// </summary>
public struct BoneTransform
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public BoneTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
}
[RequireComponent(typeof(Animator))]
public class RobotAnimator : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] RobotMode robot;
    Animator animator;

    readonly int speedHash = Animator.StringToHash("Speed");
    readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    readonly int isFallingHash = Animator.StringToHash("IsFalling");
    
    readonly int isRobotHash = Animator.StringToHash("IsRobot");

    bool isRobot = true;
    
    Dictionary<Transform, BoneTransform> boneTransformDict = new Dictionary<Transform, BoneTransform>();

    void Start() 
    {
        animator = GetComponent<Animator>();
            
        robot.OnJump += HandleJump;
        robot.OnFall += HandleFall;
        robot.OnLand += HandleLand;
        playerController.OnTransform += HandleTransform;
      
        //Save our bone transforms in the "idle" state
        SaveBoneTransforms();
        
        
    }
    //Because the transforming animtions dont account for every bone, using them causes the model to deform permanentely
    //To fix this we save ALL the bone transforms and load back to them after each transform anim has completed
    //I hate it too i know
    void SaveBoneTransforms()
    {
        Transform[] boneTransforms = transform.GetComponentsInChildren<Transform>();

        foreach (var tr in boneTransforms)
        {
            BoneTransform boneTransform = new BoneTransform(tr.localPosition,tr.localRotation,tr.localScale);
            boneTransformDict.Add(tr, boneTransform);                                                                        
        }
    }
    public void LoadBoneTransforms()
    {
        foreach (var entry in boneTransformDict)
        {
            entry.Key.localPosition = entry.Value.position;
            entry.Key.localRotation = entry.Value.rotation;
            entry.Key.localScale = entry.Value.scale;
            print("aaaaaaaaaa");
        }
    }


    void Update() 
    {
        animator.SetFloat(speedHash, robot.GetInputVelocityLastFrame().magnitude,0.1f,Time.deltaTime);
    }
    void HandleTransform(Vector3 momentum)
    {
        
        if (isRobot)
        {
            animator.SetBool(isRobotHash, false);
            isRobot = false;
            
        }
        else
        {
            LoadBoneTransforms();
            animator.SetBool(isRobotHash, true);
            isRobot = true;
        }
        
    }
    void HandleJump(Vector3 momentum)
    {
        animator.SetBool(isJumpingHash, true);
    }
    void HandleFall(Vector3 momentum)
    {
        animator.SetBool(isJumpingHash, false);
        animator.SetBool(isFallingHash, true);
    }
    void HandleLand(Vector3 momentum)
    {
        animator.SetBool(isFallingHash, false);
    }
    
    
}