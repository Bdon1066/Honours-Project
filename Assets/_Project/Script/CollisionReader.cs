using System;
using UnityEngine;


public class CollisionReader : MonoBehaviour
{
    public event Action<Collision> OnCollision = delegate { };
    
    void OnCollisionEnter(Collision other)
    {
        OnCollision.Invoke(other);
    }
}

