using UnityEngine;

/// <summary>
/// The Mover Interface, all movers (i.e. car, robot) will implement this interface.
/// Movers handle how each Mode will move, actual movement in the world is done via Mode class
/// </summary>
public interface IMover
{
    /// <summary>
    /// This function is used by modes, to make ground checks using the movers sensor
    /// </summary>
    //public void CheckForGround();
    /// <summary>
    /// This function is used by modes,to check if grounded
    /// </summary>
    //public bool IsGrounded();
    /// <summary>
    /// This function is used by modes,to get ground normal
    /// </summary>
    //public Vector3 GetGroundNormal();
    /// <summary>
    /// This function is used by modes, to set the movers rigidbody velocity
    /// </summary>
    //public void SetVelocity(Vector3 velocity);
    /// <summary>
    /// This function is used by modes, to set if we should extend the movers sensor more
    /// </summary>
   // public void SetExtendSensorRange(bool isExtended);
    
    /// <summary>
    ///  //For Initializing Mover, used instead of Awake().This is called by a mode (i.e RobotMode).
    /// </summary>
    public void Init();
    /// <summary>
    /// For enabling and disabling movers, so that only one at a time active
    /// </summary>
    public bool IsEnabled();
    /// <summary>
    /// For enabling and disabling movers, so that only one at a time active 
    /// </summary>
    /// <param name="value">If the mover should be enabled</param>
    public void SetEnabled(bool value);

}
public class BaseMover : MonoBehaviour, IMover
{

    bool isEnabled;
    public virtual void Init()
    {
        
    }
    public bool IsEnabled() => isEnabled;
   
    public virtual void SetEnabled(bool value)
    {
        isEnabled = value;
    }
}
