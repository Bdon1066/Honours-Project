using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Mode Interface, all modes (i.e. car, robot) will implement this interface.
/// The mode class handles each form's movement, animations, and is self-contained.
/// </summary>
public interface IMode
{
    public void Init(InputReader inputReader);
    
    public void ShowModel();
    public void HideModel();

    public Vector3 GetMomentum();
    public IState GetState();
    
    public void EnterMode(IState entryState, Vector3 entryMomentum);
}

public abstract class BaseMode : MonoBehaviour, IMode, IMovementStateController
{
    protected Transform tr;
    protected InputReader input;
    [Header("Base Mode")]
    [SerializeField] protected GameObject model;
    protected StateMachine stateMachine;
    
    
    protected Vector3 momentum;
    public bool useLocalMomentum;
    protected virtual void Awake()
    {
        tr = transform;
        SetupStateMachine();
    }
    

    protected abstract void SetupStateMachine();

    public abstract void Init(InputReader inputReader);
    
    public void ShowModel() => model.SetActive(true);
    public void HideModel() => model.SetActive(false);
    public Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
    
    public bool IsGrounded() => stateMachine.CurrentState is GroundedState or SlidingState;
    
    protected void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    protected void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    
    protected bool IsRising() => Utils.GetDotProduct(GetMomentum(), tr.up) > 0f;
    protected bool IsFalling() => Utils.GetDotProduct(GetMomentum(), tr.up) < 0f;

    public IState GetState()
    {
        if (stateMachine != null)
        {
            return stateMachine.CurrentState;
        }
        return null;

    }
    protected virtual void Update()
    {
        stateMachine.Update();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    public abstract void EnterMode(IState entryState, Vector3 entryMomentum);
    public abstract void EnterMode(Vector3 entryMomentum);
    public abstract void ExitMode();

}
