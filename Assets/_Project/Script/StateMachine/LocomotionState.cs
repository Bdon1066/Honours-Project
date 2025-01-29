using UnityEngine;

public class LocomotionState : BaseState
{
    public LocomotionState(IStateController controller, Animator animator) : base(controller, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFade(LocomotionHash,crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        controller.HandleLocomotion();
    }
}
