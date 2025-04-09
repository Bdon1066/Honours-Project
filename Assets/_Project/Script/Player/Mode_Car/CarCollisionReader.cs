using System;
using UnityEngine;


public class CarCollisionReader : MonoBehaviour
{
    
    public event Action<Collision> OnCollision = delegate { };
    void Awake()
    {
    }

    void OnCollisionEnter(Collision other)
    {
        OnCollision.Invoke(other);
       
    }
}

