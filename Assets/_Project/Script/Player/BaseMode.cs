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
    public void SetPosition(Vector3 position);
    public abstract void EnterMode(Vector3 entryVelocity, Vector3 entryDirection);
    public abstract void TransformTo(Vector3 momentum);
    public abstract void TransformFrom(Vector3 momentum);
    public void ExitMode();
}

public abstract class BaseMode : MonoBehaviour, IMode
{
    public abstract void Init(PlayerController playerController);
    /// <summary>
    /// Enter Mode is called when transforming has finished and we have began play in this mode
    /// </summary>
    /// <param name="entryVelocity"></param>
    /// <param name="entryDirection"></param>
    public abstract void EnterMode(Vector3 entryVelocity, Vector3 entryDirection);
    /// <summary>
    /// Called when we begin transforming Into this mode
    /// </summary>
    /// <param name="momentum"></param>
    public abstract void TransformTo(Vector3 momentum);
    /// <summary>
    /// Called when we begin transforming Out of this mode
    /// </summary>
    /// <param name="momentum"></param>
    public abstract void TransformFrom(Vector3 momentum);
    /// <summary>
    /// Exit mode is called when transforming has finished and we have stopped play in this mode
    /// </summary>
    public abstract void ExitMode();
    
    public abstract Vector3 GetVelocity();
    public abstract Vector3 GetDirection();
    public abstract void SetPosition(Vector3 position);
    
}
