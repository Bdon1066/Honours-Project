using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]  
public class CarSphereMotor : MonoBehaviour
{
   [HideInInspector] public Rigidbody rb;
   [HideInInspector] public Transform tr;
   [HideInInspector] public SphereCollider col;


   void Awake()
   {
       rb = GetComponent<Rigidbody>();
       tr = GetComponent<Transform>();
       col = GetComponent<SphereCollider>();
   }
   void OnValidate()
   {
      Awake();
   }
   
  
}
