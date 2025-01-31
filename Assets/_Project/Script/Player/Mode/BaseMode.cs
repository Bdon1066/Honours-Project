using System;
using UnityEngine;

/// <summary>
/// The Mode Interface, all modes (i.e. car, robot) will implement this interface.
/// The mode class handles each form's movement, animations, and is self-contained.
/// </summary>
public interface IMode
{
    /// <summary>
    /// For passing Input into each mode from player controller
    /// </summary>
    /// <param name="input">The input reader of player controller</param>
    public void Init(InputReader input);

    public Vector3 GetMomentum();
}
