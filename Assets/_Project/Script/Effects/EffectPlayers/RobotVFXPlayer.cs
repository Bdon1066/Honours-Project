using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public class RobotVFXPlayer : MonoBehaviour
{
    [SerializeField] GameObject landVFX;
    [SerializeField] GameObject smokeVFX;
    [SerializeField] GameObject wallStepVFX;
    [SerializeField] Transform wallStepLeft;
    [SerializeField] Transform wallStepRight;
    public RobotMode robot;

  
   
    void Start()
    {
        robot.OnLand += HandleLand;
        robot.OnWall += HandleWall;
    }

    private void HandleWall(Vector3 vector)
    {
       //var effect = Instantiate(wallStepVFX,robot.wallSpring.ContactPosition() + 0.01f * robot.groundSpring.ContactNormal(), Quaternion.Euler(0, 90f, 0.0f));
       //effect.transform.localScale = new Vector3(10, 10, 10);
    }

    private void HandleLand(LandForce landForce)
    {
        switch (landForce)
        {
            case LandForce.Heavy:
                HeavyLandEffects();
                break;
            case LandForce.Medium:
                MediumLandEffects();
                break;
            case LandForce.Light:
                LightLandEffects();
                break;
        }
    }

    public void HandleLeftWallStepVFX()
    {
        //Instantiate(wallStepVFX, wallStepLeft);
        //print("LEFT");
    }
    public void HandleRightWallStepVFX()
    {
        //Instantiate(wallStepVFX,wallStepRight);
       // print("RIGHT");
    }

    private void LightLandEffects()
    {
        Vector3 position = robot.groundSpring.ContactPosition() + 0.01f * robot.groundSpring.ContactNormal() + 1f * robot.transform.forward;
        Quaternion smokeRotation = Quaternion.Euler(270f, 0.0f, 0.0f);
        var effect = Instantiate(smokeVFX, position, smokeRotation);
        effect.transform.localScale = new Vector3(3, 3, 3);
    }

    private void MediumLandEffects()
    {
        Vector3 position = robot.groundSpring.ContactPosition() + 0.01f * robot.groundSpring.ContactNormal() + 1f * robot.transform.forward;
        Quaternion smokeRotation = Quaternion.Euler(270f, 0.0f, 0.0f);
        Instantiate(smokeVFX, position, smokeRotation);
    }

    private void HeavyLandEffects()
    {
        //rotate vfx to look up around the normal of the ground
        Quaternion rotation = Quaternion.LookRotation(-robot.groundSpring.ContactNormal());
        
        //position effect slightly above ground and forward of the player 
        Vector3 position = robot.groundSpring.ContactPosition() + 0.01f * robot.groundSpring.ContactNormal() + 0.01f * robot.transform.forward;
        Instantiate(landVFX, position, rotation);
    
        //play smoke effect
        Quaternion smokeRotation = Quaternion.Euler(270f, 0.0f, 0.0f);
        Instantiate(smokeVFX, position, smokeRotation);
        
    }

}
