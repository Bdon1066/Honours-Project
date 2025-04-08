using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;


public class RobotAudioPlayer : MonoBehaviour
{
    public RobotMode robotMode;
    
    public RobotAudioSheet robotAudio;

    EventInstance Jump;
    EventInstance Land;
    EventInstance Footsteps;
    EventInstance Transform;
    
    void Awake()
    {
        robotMode.OnJump += HandleJump;
        robotMode.OnLand += HandleLand;
        robotMode.ToCar += HandleTransformToCar;
        
        
       //RuntimeManager.AttachInstanceToGameObject(Jump, robotMode.gameObject);
       //RuntimeManager.AttachInstanceToGameObject(Land, robotMode.gameObject);
       //RuntimeManager.AttachInstanceToGameObject(Footsteps, robotMode.gameObject);
       //RuntimeManager.AttachInstanceToGameObject(Transform, robotMode.gameObject);
    }

    private void HandleTransformToCar()
    {
        //Transform.start();
       // Transform.release();
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.transform, robotMode.gameObject);
    }

    private void HandleLand(Vector3 velocity)
    {
        //EventInstance landInstance = FMODUnity.RuntimeManager.CreateInstance(robotAudio.land);
        //print(velocity);
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.land, robotMode.gameObject);
        //RuntimeManager.AttachInstanceToGameObject(landInstance, GetComponent<Transform>(), robotMode.gameObject.GetComponent<Rigidbody>());
    }

    private void HandleJump(Vector3 velocity)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.jump,  robotMode.gameObject);
    }

    public void HandleFootstep()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(robotAudio.footsteps,  robotMode.gameObject);
    }

    void Update()
    {
        
    }
}