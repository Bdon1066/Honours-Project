using UnityEngine;

public class JumpState : BaseState
{
    public JumpState(IStateController controller, Animator animator) : base(controller, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFade(JumpHash, crossFadeDuration);
    }
    public override void FixedUpdate()
    {
        controller.HandleJump();
        controller.HandleLocomotion();
    }
}