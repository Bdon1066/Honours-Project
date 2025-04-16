using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class ImpactAudioPlayer : MonoBehaviour
{

    public EventReference impactEvent;
    
    public float minImpactSpeed = 1f;
    public float maxImpactSpeed = 30f;
    
    EventInstance Impact;
    public GameObject ImpactEffect;
    
    private void OnCollisionEnter(Collision other)
    {
        HandleCollison(other);
    }

    private void HandleCollison(Collision other)
    {
        var impactSpeed = other.relativeVelocity.magnitude;
        
        //if impact velocity lower than min, return and do not play
        if (other.relativeVelocity.magnitude < minImpactSpeed) return;
        
        float normalizedImpactSpeed = Mathf.Clamp01(Mathf.Abs(impactSpeed) / maxImpactSpeed);
        Impact = RuntimeManager.CreateInstance(impactEvent);
      
        //set impact to trigger at contact point
        Impact.set3DAttributes(other.GetContact(0).point.To3DAttributes());
        
        Impact.setParameterByName("Speed", normalizedImpactSpeed);

        Impact.start();
        Instantiate(ImpactEffect, other.GetContact(0).point,Quaternion.identity);

    }
    

}