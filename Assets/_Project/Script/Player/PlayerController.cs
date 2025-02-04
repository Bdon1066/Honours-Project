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

    BaseMode currentMode;
    BaseMode previousMode; 

    List<BaseMode> modes = new List<BaseMode>();

    bool transformInputLocked, transformWasPressed, transformLetGo, transformIsPressed;

    CountdownTimer transformTimer;

    public event Action<Vector3> OnTransform = delegate { };

    BaseMode GetCurrentMode() => currentMode;
    BaseMode GetPreviousMode() => currentMode;
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

    public void OnModeStart<T>() where T : BaseMode
    {
        print("Starting Mode:  "+ typeof(T).Name);
        SetMode<T>();
        if (previousMode != null) currentMode.EnterMode(previousMode.GetState(),previousMode.GetMomentum());
        else currentMode.EnterMode(Vector3.zero);
        //TODO: Set state of mode dependent on the state of the previous mode
    }

    public void OnTransformStart()
    {
        transformInputLocked = true;
        previousMode = currentMode;
        transformTimer.Start();

        OnTransform.Invoke(currentMode.GetMomentum());
    }

    public void OnModeExit<T>() where T : BaseMode
    {
        
    }


    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    

    private void InitModes()
    {
        GetComponents(modes);

        foreach (var mode in modes)
        {
            mode.Init(input);
            SetMode<RobotMode>();
        }

        
    }
    BaseMode GetMode<T>() where T : BaseMode
    {
        foreach (var mode in modes)
        {
            if (mode is T) return mode;
        }
        return null;
    }
    void SetMode<T>() where T : BaseMode
    {
        currentMode = GetMode<T>();
        foreach (var mode in modes)
        {
            //enable our set mode and disable all others
            if (mode is T)
            {
                //mode.SetEnabled(true);
                mode.enabled = true;
                mode.ShowModel();
            }
            else
            {
                mode.enabled = false;
                mode.HideModel();
            }
        }
        
    }

}
