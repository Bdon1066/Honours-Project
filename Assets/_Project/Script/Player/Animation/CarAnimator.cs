using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAnimator : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] CarMode car;

    public GameObject[] brakeLights = new GameObject[2];
    public Transform[] visualWheels = new Transform[4];

    public float wheelVisualTurnSpeed = 5f;
    void LateUpdate()
    {
        if (car.IsBraking)
        {
            brakeLights[0].SetActive(true);
            brakeLights[1].SetActive(true);
        }
        else
        {
            brakeLights[0].SetActive(false);
            brakeLights[1].SetActive(false);
        }

        for (int i = 0; i < car.axles.Length; i++)
        {
            if (car.axles[i].steering)
            {
                visualWheels[0].transform.localRotation = Quaternion.Slerp(visualWheels[0].transform.localRotation, car.axles[i].leftWheel.tr.localRotation,wheelVisualTurnSpeed * Time.fixedDeltaTime);
                visualWheels[1].transform.localRotation = Quaternion.Slerp(visualWheels[1].transform.localRotation, car.axles[i].rightWheel.tr.localRotation,wheelVisualTurnSpeed * Time.fixedDeltaTime);
            }
            if (car.axles[i].motor)
            {
                //visualWheels[0].transform.localRotation
            }
            
           

        }


    }
}
    
   
