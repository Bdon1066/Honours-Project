using System;
using UnityEngine;

public interface IMover
{
    public bool IsGrounded();
    public Vector3 GetGroundNormal();
    public void SetVelocity(Vector3 velocity);
    public void SetExtendSensorRange(bool isExtended);
    
    public void CheckForGround();
   
}
/// <summary>
/// The base mover class which handles ground raycasting, colliding and moving the player rigidbody
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))] 
public abstract class BaseMover : MonoBehaviour, IMover
{
    protected Rigidbody rb;
    protected Transform tr;
    protected CapsuleCollider col;
    protected RaycastSensor sensor;
    
    protected bool isGrounded;
    protected float baseSensorRange;
    protected Vector3 currentGroundAdjustmentVelocity; //The velocity needed to adjust the player pos to correct ground pos over one frame
    protected int currentLayer;
    
    protected bool usingExtendedSensorRange = true; //For use pn uneven terrain
    
   
    public bool IsGrounded() => isGrounded;
    public Vector3 GetGroundNormal() => sensor.GetNormal();
    
    public void SetVelocity(Vector3 velocity) => rb.velocity = velocity + currentGroundAdjustmentVelocity;
    
    public void ConstrainRigidBody(RigidbodyConstraints constraints) => rb.constraints = constraints;
    public void SetExtendSensorRange(bool isExtended) => usingExtendedSensorRange = isExtended;
    
    public abstract void CheckForGround();
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        col = GetComponent<CapsuleCollider>();
       
    }

    
}