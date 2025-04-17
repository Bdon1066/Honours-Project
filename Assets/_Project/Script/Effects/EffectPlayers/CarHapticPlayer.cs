using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarHapticPlayer : MonoBehaviour
{
    public CarMode car;
    public CarHapticSheet effects;
    public CollisionReader carCollision;


    void Start()
    {
        //robot.OnLand += HandleLand;
        car.ToRobot += HandleTransform;
        car.OnEnter += HandleCarEnter;
        carCollision.OnCollision += HandleCollison;

        effects.Init();

    }

    private void HandleCollison(Collision collision)
    {
        if (!car.isEnabled) return;
        effects.impact.Start();
    }

    private void HandleCarEnter()
    {
       effects.engine.Start();
        print("StartEngine");
    }

    private void HandleTransform()
    {
        effects.transform.Start();
        effects.engine.Stop();
    }

    private void Update()
    {
        foreach (var effect in effects.activeEffects)
        {
            effect.Tick();
            
           
        }
        if (car.IsGrounded())
        {
            effects.engine.lowSpeedIntesity = car.normalizedSpeed;
        }

       
    }
    private void HandleLand(LandForce force)
    {
        
    }
    public void HandleFootstepHaptics()
    {
       // effects.footstepEffect.Start();
    }

    private void OnApplicationQuit()
    {
        Gamepad.current.ResetHaptics();
    }
}
