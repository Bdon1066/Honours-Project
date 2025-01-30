using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.XR;

public class StateMachine
{
    StateNode current;
    Dictionary<Type, StateNode> nodes = new();
    HashSet<ITransition> anyTransitions = new(); //transitions that dont need a "From" state

    public IState CurrentState => current.State;
    
    public void Update()
    {
        var transition = GetTransition();
        if (transition != null) ChangeState(transition.To);
        
        current.State?.Update();
    }
    public void FixedUpdate()
    {
        current.State?.FixedUpdate();
    }
    public void SetState(IState state)
    {
        current = nodes[state.GetType()];
        current.State.OnEnter();
    }
    public void ChangeState(IState state)
    {
        if (state == current.State) return;

        var previousState = current.State;
        var nextState = nodes[state.GetType()].State;

        previousState?.OnExit();
        nextState?.OnEnter();
        current = nodes[state.GetType()];

    }
    ITransition GetTransition()
    {
        foreach (var transition in anyTransitions) //check any transitions
        {
            if (transition.Condition.Evaluate()) return transition; //if transition conditions are met, return this transition
        }

        foreach (var transition in current.Transitions) //check current state specific transitions
        {
            if (transition.Condition.Evaluate()) return transition; //like above
        }

        return null;
    }
    public void AddTransition(IState from, IState to, IPredicate condition)
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition); //create From node, add transition that goes to the To node, based on Condition
    }
    public void AddAnyTransition(IState to, IPredicate condition)
    {
        anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition)); // add an any transition that goes to the To node, based on Condition
    }
    
    
    StateNode GetOrAddNode(IState state)
    {
        var node = nodes.GetValueOrDefault(state.GetType());

        if (node == null)
        {
            node = new StateNode(state);
            nodes.Add(state.GetType(),node);
        }

        return node;
    }

    
    class StateNode
    {
        public IState State { get;  }
        public HashSet<ITransition> Transitions { get; }

        public StateNode(IState state)
        {
            State = state;
            Transitions = new HashSet<ITransition>();
        }
        public void AddTransition(IState to, IPredicate condition)
        {
            Transitions.Add(new Transition(to, condition));
        }
    }
}
