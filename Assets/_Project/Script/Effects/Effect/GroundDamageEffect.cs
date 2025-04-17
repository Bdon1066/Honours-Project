using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;

public class GroundDamageEffect : MonoBehaviour
{
    public Texture[]  groundDamageTextures;
    Material material;
    //private static readonly int ExpandHash = Animator.StringToHash("Jumping");
    
    Animator animator;
    // Start is called before the first frame update
    

    void Awake()
    {
        material = GetComponent<Renderer>().material;
        material.mainTexture = groundDamageTextures[Random.Range(0, groundDamageTextures.Length)];
        //Quaternion rotation = Quaternion.AngleAxis(Random.Range(0, 360), Ve.up);
        //Quaternion randomRotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        //transform.rotation = rotation;
        //transform.rotation.eulerAngles.x
    }
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}
