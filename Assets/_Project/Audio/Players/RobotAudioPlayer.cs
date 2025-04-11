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
    public CollisionReader robotCollision;
    
    public float minImpactSpeed = 1f;
    public float maxImpactSpeed = 30f;

    bool playAudio = true;
    void Awake()
    {
        robotMode.OnJump += HandleJump;
        robotMode.OnLand += HandleLand;
        robotMode.ToCar += HandleTransformToCar;
        robotMode.ToRobot += HandleTransformToRobot;
        
        robotCollision.OnCollision += HandleCollision;
    }

    private void HandleCollision(Collision other)
    {
        //var impactSpeed = other.relativeVelocity.magnitude;
        
        //if impact velocity lower than min, return and do not play
        //if (other.relativeVelocity.magnitude < minImpactSpeed) return;
        
        //float normalizedImpactSpeed = Mathf.Clamp01(Mathf.Abs(impactSpeed) / maxImpactSpeed);
        
    }

    private void HandleTransformToRobot()
    {
        playAudio = true;
        robotMode.OnJump += HandleJump;
        robotMode.OnLand += HandleLand;
    }

    private void HandleTransformToCar()
    {
        PlayOneShot(robotAudio.transform, robotMode.gameObject);
        robotMode.OnJump -= HandleJump;
        robotMode.OnLand -= HandleLand;
        playAudio = false;
    }

    private void HandleLand(Vector3 velocity)
    {
        PlayOneShot(robotAudio.land, footstepPosition.gameObject.transform.position);
    }

    private void HandleJump(Vector3 velocity)
    {
        PlayOneShot(robotAudio.jump,  robotMode.gameObject);
    }

    public void HandleFootstep()
    {
        if (robotMode.GetVelocity().magnitude < 0.1f)  return;
        PlayOneShot(robotAudio.footsteps, footstepPosition.gameObject);
    }

    void PlayOneShot(EventReference eventRef,GameObject gameObject)
    {
        if (!playAudio) return;
        RuntimeManager.PlayOneShotAttached(eventRef,gameObject);
        //print("Playing audio: "+ eventRef);
    }
    void PlayOneShot(EventReference eventRef,Vector3 position)
    {
        if (!playAudio) return;
        RuntimeManager.PlayOneShot(eventRef,position);
        //print("Playing audio: "+ eventRef);
    }
}