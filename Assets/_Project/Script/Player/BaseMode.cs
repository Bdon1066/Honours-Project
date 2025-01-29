using UnityEngine;

/// <summary>
/// The Mode Interface, all modes (i.e. car, robot) will implement this interface.
/// The mode class handles each form's movement, animations, and is self-contained.
/// </summary>
public interface IMode 
{
    
}
public abstract class BaseMode: MonoBehaviour, IStateController, IMode
{
   
}