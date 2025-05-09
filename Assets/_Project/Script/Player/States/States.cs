using UnityEditor;
/// <summary>
/// A state when we are on the ground and can move via walking or running
/// </summary>
public class GroundedState : IState
{
    readonly IMovementStateController controller;
    public GroundedState(IMovementStateController controller)
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
    readonly IMovementStateController controller;
    public FallingState(IMovementStateController controller)
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
    readonly IMovementStateController controller;
    public SlidingState(IMovementStateController controller)
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
    readonly IMovementStateController controller;
    public RisingState(IMovementStateController controller)
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
    readonly IMovementStateController controller;
    public JumpingState(IMovementStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
        controller.OnJumpStart();
    }

}
public class WallState : IState
{
    readonly IMovementStateController controller;
    public WallState(IMovementStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnWallStart();
    }

}
public class ClimbEndState : IState
{
    readonly IMovementStateController controller;
    public ClimbEndState(IMovementStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnClimbEnd();
    }

}
public class WallJumpingState : IState
{
    readonly IMovementStateController controller;
    public WallJumpingState(IMovementStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnWallJumpStart();
    }

}
public class HeavyLandedState : IState
{
    readonly IMovementStateController controller;
    public HeavyLandedState(IMovementStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactRegained();
        controller.OnHeavyLandStart();
    }

}
/// <summary>
/// A state when we have finihsed transforming and need to slide with our previous modes velocity
/// </summary>
public class PostTransformState : IState
{
    readonly IMovementStateController controller;
    public PostTransformState(IMovementStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {

    }

}
/// <summary>
/// A state for when we are in Robot Mode
/// </summary>
public class RobotState : IState
{
    readonly IModeStateController controller;
    public RobotState(IModeStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnModeStart<RobotMode>();
    }

    public void OnExit()
    {
        controller.OnModeExit<RobotMode>();
    }

}
/// <summary>
/// A state for when we are in Car Mode
/// </summary>
public class CarState : IState
{
    readonly IModeStateController controller;
    public CarState(IModeStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnModeStart<CarMode>();
    }

}
public class TransformingState : IState
{
    readonly IModeStateController controller;
    public TransformingState(IModeStateController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnTransformStart();
    }
    public void OnExit()
    {
        controller.OnTransformExit();
    }

}
