using UnityEngine;

/// <summary>
/// The Mover Interface, all movers (i.e. car, robot) will implement this interface.
/// Movers handle how each Mode will move, actual movement in the world is done via Mode class
/// </summary>
public interface IMover
{
    
    /// <summary>
    ///  //For Initializing Mover, used instead of Awake().This is called by a mode (i.e RobotMode).
    /// </summary>
    public void Init();
    

    public void Enable();
    public void Disable();
}
public abstract class BaseMover : MonoBehaviour, IMover
{

    public abstract void Init();
    public abstract void Enable();
    public abstract void Disable();

}

