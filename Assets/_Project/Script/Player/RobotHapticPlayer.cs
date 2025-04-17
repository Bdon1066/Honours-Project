using System;
using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotHapticPlayer : MonoBehaviour
{
    public RobotMode robot;
    public HapticEffect heavyLandEffect;

   

    void Start()
    {
        robot.OnLand += HandleLand;
        heavyLandEffect.Init();
    }

    private void HandleLand(LandForce force)
    {
        if (force == LandForce.Heavy)
        {
            heavyLandEffect.Start();
        }
    }
}
