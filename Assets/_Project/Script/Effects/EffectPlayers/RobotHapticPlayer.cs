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
        effects.Init();
       
    }

    private void HandleWall(Vector3 vector)
    {
        effects.mediumLandEffect.Start();
    }

    private void HandleTransform()
    {
       effects.transform.Start();
    }

    private void Update()
    {
        foreach (var effect in effects.activeEffects)
        {
            effect.Tick();

        }
    }
    private void HandleLand(LandForce force)
    {
        switch (force)
        {
            case LandForce.Heavy:
                effects.heavyLandEffect.Start();
                break;
            case LandForce.Medium:
                effects.mediumLandEffect.Start();
                break;
            case LandForce.Light:
                effects.lightLandEffect.Start(); 
                break;
        }
    }
    public void HandleFootstepHaptics()
    {
        if (robot.GetVelocity().magnitude < 0.1f) return;
        effects.footstepEffect.Start();
    }
    public void HandleWallstepHaptics()
    {
        if (robot.GetVelocity().magnitude < 0.1f) return;
        effects.wallstepEffect.Start();
    }

    private void OnApplicationQuit()
    {
        Gamepad.current.ResetHaptics();
    }
}
