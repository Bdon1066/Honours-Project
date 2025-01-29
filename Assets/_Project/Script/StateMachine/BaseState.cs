using UnityEngine;

public abstract class BaseState : IState
{
    protected readonly IStateController controller;
    protected readonly Animator animator;

    protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    protected static readonly int JumpHash = Animator.StringToHash("Locomotion");

    protected const float crossFadeDuration = 0.1f;

    protected BaseState(IStateController controller, Animator animator)
    {
        this.controller = controller;
        this.animator = animator;
    }
    
    public virtual void OnEnter()
    {
        //noop
    }
    public virtual void Update()
    {
        //noop
    }
    public virtual void FixedUpdate()
    { 
        //noop
    }
    public virtual void OnExit()
    {
        //noop
    }

   
}