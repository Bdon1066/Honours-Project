using UnityEditor;
/// <summary>
/// A state when we are on the ground and can move via walking or running
/// </summary>
public class GroundedState : IState
{
    readonly IStateController controller;
    public GroundedState(IStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactRegained();
    }

}
/// <summary>
/// A state where we are falling, moving downwards while in air
/// </summary>
public class FallingState : IState
{
    readonly IStateController controller;
    public FallingState(IStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnFallStart();
    }

}
/// <summary>
///A state where we are sliding, moving downwards at an angle while grounded on steep ground
/// </summary>
public class SlidingState : IState
{
    readonly IStateController controller;
    public SlidingState(IStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
    }

}
/// <summary>
/// A state where we are rising, moving upwards while in air
/// </summary>
public class RisingState : IState
{
    readonly IStateController controller;
    public RisingState(IStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
    }

}
/// <summary>
/// A state when we are jumping, moving upwards after pressing the jump button
/// </summary>
public class JumpingState : IState
{
    readonly IStateController controller;
    public JumpingState(IStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
        controller.OnJumpStart();
    }

}
