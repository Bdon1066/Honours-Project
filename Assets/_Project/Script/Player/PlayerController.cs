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
        //initialize our modes on start and set up the mode state machine
        InitModes();
        SetupStateMachine();
        
        input.EnablePlayerActions();
        input.Transform += HandleTransformInput;
    }

    void HandleTransformInput(bool isButtonPressed)
    {
        //update the transform input flags with fresh input data
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
    }

    private void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var robot = new RobotState(this);
        var car = new CarState(this);
        var transforming = new TransformingState(this);

        //begin transforming from any mode state once input is/was pressed and isn't locked
        Any(transforming, new FuncPredicate(() => (transformIsPressed || transformWasPressed) && !transformInputLocked));
        //transform into robot when we are a car and vice versa 
        At(transforming, robot, new FuncPredicate(() => transformTimer.IsFinished && currentMode is CarMode));
        At(transforming, car, new FuncPredicate(() => transformTimer.IsFinished && currentMode is RobotMode));

        stateMachine.SetState(robot);
    }

    public void OnModeStart<T>() where T : IMode
    {
        SetMode<T>();
        if (previousMode != null && previousMode.GetState() != null)
        {
            currentMode.EnterMode(previousMode.GetState(),previousMode.GetMomentum());
        }
        else
        {
            currentMode.EnterMode();
        }
       //TODO: Set state of mode dependent on the state of the previous mode
    }
    public void OnModeExit<T>() where T : IMode
    {
        currentMode.ExitMode();
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
            SetMode<RobotMode>();
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
    void SetMode<T>() where T : IMode
    {
        currentMode = GetMode<T>();
        foreach (var mode in modes)
        {
            //enable our set mode and disable all others
            if (mode is T)
            {
                mode.SetEnabled(true);
                mode.ShowModel();
            }
            else
            {
                mode.SetEnabled(false);
                mode.HideModel();
            }
        }
        
    }

}
