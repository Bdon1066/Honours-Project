using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAnimator : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] CarMode car;
    
    public GameObject[] brakeLights = new GameObject[2];
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
    }
}
