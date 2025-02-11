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
    
    public void ShowModel();
    public void HideModel();

    public Vector3 GetMomentum();

    public Vector3 GetMovementVelocity();
    
    public void SetEnabled(bool value);
    public bool IsEnabled();

    /// <summary>
    /// This function is called when this mode is entered (i.e transformed into) via the PlayerController.
    /// </summary>
    /// <param name="entryState">The state we want this mode to start in</param>
    /// <param name="entryMomentum">The momementum we want this mode to start with</param>
    public void EnterMode(Vector3 entryMomentum);
    /// <summary>
    /// This function is called when this mode is entered (i.e transformed into)via the PlayerController. This overload is a default for the first mode entry at game start
    /// </summary>
    public void EnterMode();
    /// <summary>
    /// This function is called when this mode is exited (i.e transformed out of)via the PlayerController.
    /// </summary>
    public void ExitMode();
}

public abstract class BaseMode : MonoBehaviour, IMode, IMovementStateController
{

    public abstract void Init(PlayerController playerController);
    
    public abstract void EnterMode(Vector3 entryMomentum);
    public abstract void EnterMode();
    public abstract void ExitMode();
    public abstract void ShowModel();
    public abstract void HideModel();
    public abstract Vector3 GetMomentum();
    public abstract Vector3 GetMovementVelocity();
    public abstract void SetEnabled(bool value);
    public abstract bool IsEnabled();
    
}
