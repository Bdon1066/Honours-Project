using System;
using UnityEngine;

/// <summary>
/// The Mode Interface, all modes (i.e. car, robot) will implement this interface.
/// The mode class handles each form's movement, animations, and is self-contained.
/// </summary>
public interface IMode
{
    /// <summary>
    ///  //For Initializing Mode with our player controllers input, used instead of Awake().This is called by PlayerController Start().
    /// </summary>
    /// <param name="input">The input reader of player controller</param>
    public void Init(PlayerController playerController);
    public Vector3 GetVelocity();
    public Vector3 GetDirection();
    public Vector3 GetMovementVelocity();
    public void SetPosition(Vector3 position);
    public abstract void EnterMode(Vector3 entryVelocity, Vector3 entryDirection);
    /// <summary>
    /// This function is called when this mode is exited (i.e transformed out of)via the PlayerController.
    /// </summary>
    public void ExitMode();
}

public abstract class BaseMode : MonoBehaviour, IMode, IMovementStateController
{

    public abstract void Init(PlayerController playerController);
    
    public abstract void EnterMode(Vector3 entryVelocity, Vector3 entryDirection);

    public abstract void ExitMode();

    public abstract Vector3 GetVelocity();
    public abstract Vector3 GetDirection();
    public abstract Vector3 GetMovementVelocity();
    
    public abstract void SetPosition(Vector3 position);


    
}
