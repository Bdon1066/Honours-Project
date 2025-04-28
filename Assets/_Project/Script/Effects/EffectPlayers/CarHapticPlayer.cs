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


    private HapticInstance Engine;

    void Start()
    {
        car.ToRobot += HandleTransform;
        car.OnEnter += HandleCarEnter;
        carCollision.OnCollision += HandleCollison;
    }

    private void HandleCollison(Collision collision)
    {
        if (!car.isEnabled) return;
        HapticManager.Instance.PlayOneShot(effects.impact);
    }

    private void HandleCarEnter()
    {
        Engine = HapticManager.Instance.Play(effects.engine);
    }

    private void HandleTransform()
    {
        HapticManager.Instance.PlayOneShot(effects.transform);
        Engine.Stop();
        Engine.Release();
    }

    private void Update()
    {
        if (car.IsGrounded())
        {
            effects.engine.lowSpeedIntesity = car.normalizedSpeed;
        }
    }
    private void HandleLand(LandForce force)
    {
        //noop
    }
    public void HandleFootstepHaptics()
    {
       // effects.footstepEffect.Start();
    }
}
