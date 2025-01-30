/// <summary>
/// An interface allowing class functions to be refrenced by states (for use in player controller and handles mode change states)
/// </summary>
public interface IStateController
{
    
}
/// <summary>
/// An interface allowing specifically movement class functions to be refrenced by states
/// </summary>
public interface IMovementStateController : IStateController
{
    public virtual void OnGroundContactRegained()
    {
        //noop
    }
    public virtual void OnGroundContactLost()
    {
        //noop
    }
    public virtual void OnFallStart()
    {
        //noop
    }
   
    public virtual void OnJumpStart()
    {
        //noop
    }

}