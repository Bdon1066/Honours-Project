using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotVFXPlayer : MonoBehaviour
{
    [SerializeField] GameObject landVFX;
    [SerializeField] GameObject smokeVFX;
    public RobotMode robot;

  
   
    void Start()
    {
        robot.OnLand += HandleLand;
    }
    private void HandleLand(LandForce landForce)
    {
        if (landForce == LandForce.Heavy)
        {
           
            Quaternion rotation = Quaternion.LookRotation(-robot.groundSpring.ContactNormal());
            Vector3 position = robot.groundSpring.ContactPosition() + 0.01f * robot.groundSpring.ContactNormal() + 0.01f * robot.transform.forward;
            Instantiate(landVFX, position,rotation);
            
            Quaternion smokeRotation =  Quaternion.Euler(270f, 0.0f, 0.0f);
            
            Instantiate(smokeVFX, position, smokeRotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
