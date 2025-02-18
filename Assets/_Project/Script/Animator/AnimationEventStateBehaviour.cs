using UnityEditor.Build;
using UnityEngine;

public class AnimationEventStateBehaviour : StateMachineBehaviour
{
    public string eventName;
    [Range(0f, 1f)] public float normalizedTime;

    bool hasTriggered;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1;

        if (!hasTriggered && currentTime >= normalizedTime)
        {
            NotifyReceiver(animator);
            hasTriggered = true;
        }
    }
    void NotifyReceiver(Animator animator)
    {
        AnimationEventReceiver receiver = animator.GetComponent<AnimationEventReceiver>();
        if (receiver != null)
        {
            receiver.OnAnimationEventTriggered(eventName);
        }
    }
}