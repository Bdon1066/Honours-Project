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
        effects.Init();
       
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
        effects.footstepEffect.Start();
    }

    private void OnApplicationQuit()
    {
        Gamepad.current.ResetHaptics();
    }
}
