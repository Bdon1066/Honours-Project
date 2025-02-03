/// <summary>
/// An interface allowing class functions to be refrenced by states (for use in player controller and handles mode change states)
/// </summary>
public interface IStateController{}
/// <summary>
/// An interface allowing specifically movement class functions to be refrenced by states
/// </summary>
public interface IMovementStateController : IStateController
{
    public void OnGroundContactRegained()
    {
        //noop
    }
    public void OnGroundContactLost()
    {
        //noop
    }
    public void OnFallStart()
    {
        //noop
    }
   
    public void OnJumpStart()
    {
        //noop
    }


}
/// <summary>
/// An interface allowing specifically mode class functions to be refrenced by states
/// </summary>
public interface IModeStateController : IStateController
{
    public void OnModeStart<T>() where T : IMode;


    public void OnTransformStart();

}