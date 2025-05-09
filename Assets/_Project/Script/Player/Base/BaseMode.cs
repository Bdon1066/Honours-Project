using System;
using UnityEngine;

/// <summary>
/// The Mode Interface, all modes (i.e. car, robot) will implement this interface.
/// The mode class handles each form's movement, animations, and is self-contained.
/// </summary>
public interface IMode
{
    public void AwakeMode(PlayerController playerController);
    public Vector3 GetVelocity();
    public void SetPosition(Vector3 position);
    public abstract void EnterMode(Vector3 entryVelocity);
    public abstract void TransformTo(BaseMode fromMode);
    public abstract void TransformFrom(BaseMode toMode);
    public void ExitMode();
}

public abstract class BaseMode : MonoBehaviour, IMode
{
    public abstract void AwakeMode(PlayerController playerController);
    /// <summary>
    /// Enter Mode is called when transforming has finished and we have began play in this mode
    /// </summary>
    /// <param name="entryVelocity"></param>
    /// <param name="entryDirection"></param>
    public abstract void EnterMode(Vector3 entryVelocity);
    /// <summary>
    /// Called when we begin transforming Into this mode
    /// </summary>
    /// <param name="momentum"></param>
    public abstract void TransformTo(BaseMode fromMode);
    /// <summary>
    /// Called when we begin transforming Out of this mode
    /// </summary>
    /// <param name="momentum"></param>
    public abstract void TransformFrom(BaseMode toMode);
    /// <summary>
    /// Exit mode is called when transforming has finished and we have stopped play in this mode
    /// </summary>
    public abstract void ExitMode();
    
    public abstract Vector3 GetVelocity();

    public abstract Transform GetRootBone();
    public abstract void SetPosition(Vector3 position);
    
}
