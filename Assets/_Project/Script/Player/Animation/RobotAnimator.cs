using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This struct is used for saving each bones transform before and after using the wacky Transforming animations that break everything
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

    static readonly int speedHash = Animator.StringToHash("Speed");
    static readonly int verticalWallSpeedHash = Animator.StringToHash("VerticalWallSpeed");
    static readonly int horizontalWallSpeedHash = Animator.StringToHash("HorizontalWallSpeed");
    
    private static readonly int JumpHash = Animator.StringToHash("Jumping");
    private static readonly int FallHash = Animator.StringToHash("Fall");
    private static readonly int LandHash = Animator.StringToHash("Land");
    private static readonly int ClimbHash = Animator.StringToHash("Climb");
    private static readonly int ClimbEndHash = Animator.StringToHash("ClimbEnd");
    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private static readonly int IdleHash = Animator.StringToHash("SwayIdle");
    private static readonly int ToCarHash = Animator.StringToHash("TransformToCar");
    private static readonly int ToRobotHash = Animator.StringToHash("TransformToRobot");

    private CountdownTimer transformTimer;
    
    Dictionary<Transform, BoneTransform> boneTransformDict = new Dictionary<Transform, BoneTransform>();

    private bool isTransforming;

    PlayerStateEvent cachedState;

    enum PlayerStateEvent {Locomotion,Fall,Land,Jump,Climb,ClimbEnd}

    void Start() 
    {
        animator = GetComponent<Animator>();
        animator.keepAnimatorStateOnDisable = true;
            
        robot.OnJump += HandleJump;
        robot.OnFall += HandleFall;
        robot.OnLand += HandleLand;
        robot.OnWall += HandleWall;
        robot.OnEndClimb += HandleEndClimb;
        
        robot.ToCar += HandleTransformToCar;
        robot.ToRobot += HandleTransformToRobot;

        //init our transform timer with the transform timer's time
        //despicable hard coded string reference thank you unity
        transformTimer = new CountdownTimer( GetAnimStateTime("transform_to_main"));
        transformTimer.OnTimerStop += OnTransformFinished;
        
        //Save our bone transforms in the "idle" state
        SaveBoneTransforms();
    }
    float GetAnimStateTime(string name)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        float time = 0;
        for (int i = 0; i < ac.animationClips.Length; i++)
        {
            if (ac.animationClips[i].name == name)
                time = ac.animationClips[i].length;
            
        }
        return time;
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
    void LoadBoneTransforms()
    {
        foreach (var entry in boneTransformDict)
        {
            entry.Key.localPosition = entry.Value.position;
            entry.Key.localRotation = entry.Value.rotation;
            entry.Key.localScale = entry.Value.scale;
        }
    }
    
    void Update() 
    {
        UpdateAnimatorParameters();
    }
    void UpdateAnimatorParameters()
    {
        Vector3 horizontalVelocity = new Vector3(robot.GetVelocity().x, 0, robot.GetVelocity().z);
        Vector3 verticalVelocity = new Vector3(0, robot.GetVelocity().y, 0);
        
        animator.SetFloat(speedHash, horizontalVelocity.magnitude,0.1f,Time.deltaTime);
        
        float horizontalVelocityFloat = horizontalVelocity.x + horizontalVelocity.z;

        //invert sign of float values when on of differing directions,hacky fix
        if (robot.transform.forward == Vector3.forward)
        {
            horizontalVelocityFloat = -horizontalVelocityFloat;
        }
        if (robot.transform.forward == -Vector3.right)
        {
            horizontalVelocityFloat = -horizontalVelocityFloat;
        }
        animator.SetFloat(verticalWallSpeedHash, verticalVelocity.y,0.1f,Time.deltaTime);
        animator.SetFloat(horizontalWallSpeedHash,horizontalVelocityFloat,0.1f,Time.deltaTime);
    }
    void PlayOrCacheAnimation(int animHash,float normalizedTransitionDuration,int layer)
    {
        if (!isTransforming)
        {
            animator.CrossFade(animHash, normalizedTransitionDuration, layer);
        }
        else
        {
            cachedState = GetStateEvent(animHash);
        }
    }

    void HandleJump(Vector3 momentum)
    {
        PlayOrCacheAnimation(JumpHash,0.1f,0);
    }
    void HandleFall(Vector3 momentum)
    {
        PlayOrCacheAnimation(FallHash,0.3f,0);
    }
    void HandleLand(Vector3 momentum)
    {
        PlayOrCacheAnimation(LocomotionHash,0.2f,0);
    }
    void HandleEndClimb()
    {
        PlayOrCacheAnimation(ClimbEndHash,0.2f,0);
    }
    void HandleWall(Vector3 velocity)
    {
        PlayOrCacheAnimation(ClimbHash,0.2f,0);
    }

    private void HandleTransformToRobot()
    {
        //reset bone transforms and animator
        LoadBoneTransforms();
        animator.Rebind();
        LoadBoneTransforms();
        
        animator.CrossFade(ToRobotHash,0,0);
        isTransforming = true;
        transformTimer.Start();
    }
    
    private void HandleTransformToCar()
    {
        //Tweak rotation to account for the run anaimtion rotating the player bones, this allows for the seamless transition
        Vector3 newRotationAngles = new Vector3(transform.rotation.eulerAngles.x, 0, transform.rotation.eulerAngles.z);
        Quaternion newRotation = Quaternion.Euler(newRotationAngles);
        transform.rotation = newRotation;
        animator.Rebind();

        isTransforming = true;
        animator.CrossFade(ToCarHash,0f,0);
    }
        
    //for caching aniamtions while transforming, so we go seamlessly from transform to say, falling, if we are falling while transforming
    private void OnTransformFinished()
    {
        isTransforming = false;
        switch (cachedState)
        {
            case PlayerStateEvent.Locomotion:
                animator.CrossFade(LocomotionHash, 0.1f, 0);
                break;
            case PlayerStateEvent.Fall:
                animator.CrossFade(FallHash, 0.1f, 0);
                break;
            case PlayerStateEvent.Land:
                animator.CrossFade(LocomotionHash, 0.1f, 0);
                break;
            case PlayerStateEvent.Jump:
                animator.CrossFade(JumpHash, 0.1f, 0);
                break;
            case PlayerStateEvent.Climb:
                animator.CrossFade(ClimbHash, 0.1f, 0);
                break;
            case PlayerStateEvent.ClimbEnd:
                animator.CrossFade(ClimbEndHash, 0.1f, 0);
                break;
        }
        cachedState = PlayerStateEvent.Locomotion;
    }
    PlayerStateEvent GetStateEvent(int animHash)
    {
        switch (animHash)
        {
            case var value when value == LocomotionHash:
                return PlayerStateEvent.Locomotion;
            case var value when value == FallHash:
                return PlayerStateEvent.Fall;
            case var value when value == LandHash:
                return PlayerStateEvent.Land;
            case var value when value == JumpHash:
                return PlayerStateEvent.Jump;
            case var value when value == ClimbHash:
                return PlayerStateEvent.Climb;
            case var value when value == ClimbEndHash:
                return PlayerStateEvent.ClimbEnd;
            default:
                return PlayerStateEvent.Locomotion;
        }
    }
}