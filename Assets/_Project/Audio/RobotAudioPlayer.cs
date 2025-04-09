using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.XR;


public class RobotAudioPlayer : MonoBehaviour
{
    public RobotMode robotMode;
    
    public RobotAudioSheet robotAudio;

    public Transform footstepPosition;

    bool playAudio = true;
    void Awake()
    {
        robotMode.OnJump += HandleJump;
        robotMode.OnLand += HandleLand;
        robotMode.ToCar += HandleTransformToCar;
        robotMode.ToRobot += HandleTransformToRobot;
        
    }

    private void HandleTransformToRobot()
    {
        robotMode.OnJump += HandleJump;
        robotMode.OnLand += HandleLand;
    }

    private void HandleTransformToCar()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.transform, robotMode.gameObject);
        robotMode.OnJump -= HandleJump;
        robotMode.OnLand -= HandleLand;
    }

    private void HandleLand(Vector3 velocity)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.land, footstepPosition.gameObject);
    }

    private void HandleJump(Vector3 velocity)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.jump,  robotMode.gameObject);
    }

    public void HandleFootstep()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.footsteps,  footstepPosition.gameObject);
    }

    void Update()
    {
        
    }
}