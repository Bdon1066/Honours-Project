using System;
using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotHapticPlayer : MonoBehaviour
{
    public RobotMode robot;
    public RobotHapticSheet effects;

    

    void Start()
    {
        robot.OnLand += HandleLand;
        robot.ToCar += HandleTransform;
        robot.OnWall += HandleWall;
       
    }

    private void HandleWall(Vector3 vector)
    {
        HapticManager.Instance.PlayOneShot(effects.mediumLandEffect);
    }

    private void HandleTransform()
    {
       HapticManager.Instance.PlayOneShot(effects.transform);
    }
    
    private void HandleLand(LandForce force)
    {
        switch (force)
        {
            case LandForce.Heavy:
                HapticManager.Instance.PlayOneShot( effects.heavyLandEffect);
                break;
            case LandForce.Medium:
                HapticManager.Instance.PlayOneShot(effects.mediumLandEffect);
                break;
            case LandForce.Light:
                HapticManager.Instance.PlayOneShot(effects.lightLandEffect);
                break;
        }
    }
    public void HandleFootstepHaptics()
    {
        if (robot.GetVelocity().magnitude < 0.1f) return;
        HapticManager.Instance.PlayOneShot(effects.footstepEffect);
    }
    public void HandleWallstepHaptics()
    {
        if (robot.GetVelocity().magnitude < 0.1f) return;
        HapticManager.Instance.PlayOneShot(effects.wallstepEffect);
    }
}
