using UnityEngine;

public interface IState
{
    void OnEnter();
    void Update();
    void FixedUpdate();
    void OnExit();
    
}
/// <summary>
/// An interface allowing this class to be referenced by states (playerController, robotMode etc.)
/// </summary>
public interface IStateController
{
    public virtual void HandleLocomotion()
    {
        //noop
    }
    public virtual void HandleJump()
    { 
        //noop
    }
}