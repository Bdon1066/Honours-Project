using System;
using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerController : MonoBehaviour, IModeStateController
{
    [SerializeField] private InputReader input;
    public float transformDuration = 1f;
    StateMachine stateMachine;

    IMode currentMode;
    IMode previousMode; 

    List<IMode> modes = new List<IMode>();

    bool transformInputLocked, transformWasPressed, transformLetGo, transformIsPressed;

    CountdownTimer transformTimer;

    public event Action<Vector3> OnTransform = delegate { };

    IMode GetCurrentMode() => currentMode;
    IMode GetPreviousMode() => currentMode;
    void Awake()
    {
        transformTimer = new CountdownTimer(transformDuration);
    }
 
    private void Start()
    {
        InitModes();
        SetupStateMachine();
        input.EnablePlayerActions();
        input.Transform += HandleTransformInput;
    }

    void HandleTransformInput(bool isButtonPressed)
    {
        if (!transformIsPressed && isButtonPressed)
        {
            transformWasPressed = true;
        }

        if (transformIsPressed && !isButtonPressed)
        {
            transformLetGo = true;
            transformInputLocked = false;
        }

        transformIsPressed = isButtonPressed;

    }
    void ResetTransformKeys()
    {
        transformLetGo = false;
        transformWasPressed = false;
    }
    void Update()
    {
        stateMachine.Update();
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
        ResetTransformKeys();

        print(stateMachine.CurrentState);
       
    }

    private void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var robot = new RobotState(this);
        var car = new CarState(this);
        var transforming = new TransformingState(this);

        Any(transforming, new FuncPredicate(() => (transformIsPressed || transformWasPressed) && !transformInputLocked));
        At(transforming, robot, new FuncPredicate(() => transformTimer.IsFinished && currentMode is CarMode));
        At(transforming, car, new FuncPredicate(() => transformTimer.IsFinished && currentMode is RobotMode));

        stateMachine.SetState(robot);
    }

    public void OnModeStart<T>() where T : IMode
    {
       SetCurrentMode<T>();

       //probs do some kickstarting to the mode
    }

    public void OnTransformStart()
    {
        transformInputLocked = true;
        previousMode = currentMode;
        transformTimer.Start();

        OnTransform.Invoke(currentMode.GetMomentum());
    }


    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    
   // bool IsTransforming() => (transformIsPressed || transformWasPressed) && !transformInputLocked;

    private void InitModes()
    {
        GetComponents(modes);

        foreach (var mode in modes)
        {
            mode.Init(input);

            if (mode is RobotMode) currentMode = mode;
        }

        
    }
    IMode GetMode<T>() where T : IMode
    {
        foreach (var mode in modes)
        {
            if (mode is T) return mode;
        }
        return null;
    }
    void SetCurrentMode<T>() where T : IMode
    {
        currentMode = GetMode<T>();
        foreach (var mode in modes)
        {
            if (mode is T) mode.ShowModel();
            else mode.HideModel();
        }
        
    }

}
