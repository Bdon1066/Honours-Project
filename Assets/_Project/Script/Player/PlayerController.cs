using System;
using ImprovedTimers;
using UnityEngine;

[System.Serializable]
public enum ModeType{Robot = 1, Car = 2}
[System.Flags]
public enum ModeTypeMask{Robot = 1, Car = 2}
public class PlayerController : MonoBehaviour, IModeStateController
{
    public InputReader input;
    public float transformDuration = 1f;
    public float transformCooldownDuration = 1f;
    StateMachine stateMachine;
    Transform tr;

    BaseMode currentMode;
    BaseMode previousMode;
    
    ModeType currentModeType;

    public BaseMode[] modes;
    
    public ModeTypeMask enabledModes;
    
    public ModeType startingMode = ModeType.Robot;
    
    //TODO: In future, we will want to change this, so we only use the enum, and we just grab thje base mdoes from some scritable object when we need it
    // very busy tho so do this before adding other modes
    // replace array with some lookup for each enabled mode and itll be way cleaner

    bool transformInputLocked,transformIsPressed,transformWasPressed,transformLetGo;

    CountdownTimer transformTimer;
    CountdownTimer transformCooldowmTimer;
    

    public event Action<ModeType, ModeType> OnTransform = delegate { };

    [Header("Debug Settings")]
    public bool debugMode;
    [Range(0, 1)] public float slowMotionRate;
    bool isPaused;
    bool isSlowMotion;
    float currentTimeScale;

    public BaseMode GetCurrentMode() => currentMode;
    BaseMode GetPreviousMode() => currentMode;
    bool IsTransforming() => stateMachine.CurrentState is TransformingState;
    void Awake()
    {
        tr = transform;
        transformTimer = new CountdownTimer(transformDuration);
        transformCooldowmTimer = new CountdownTimer(transformCooldownDuration);
        transformCooldowmTimer.OnTimerStop += HandleTransformCooldown;
        Cursor.lockState = CursorLockMode.Locked;
    }

   

    private void Start()
    {
        //initialize our modes on start and set up the mode state machine
        AwakeModes();
        SetupStateMachine();
        
        input.EnablePlayerActions();
        input.Transform += HandleTransformInput;
        input.SlowMotion += HandleSlowMotionInput;
        input.Pause += HandlePause;
    }

    private void HandlePause(bool isButtonPressed)
    {
        if (isButtonPressed && !isPaused)
        {
            currentTimeScale = Time.timeScale;
            isPaused = true;
            Time.timeScale = 0f;
        }
        else if (isButtonPressed && isPaused)
        {

            isPaused = false;
            Time.timeScale = currentTimeScale;
        }
    }

    private void HandleSlowMotionInput(bool isButtonPressed)
    {
        if (!debugMode) return;

        if (isButtonPressed && !isSlowMotion)
        {
            currentTimeScale = Time.timeScale;
            isSlowMotion = true;
            Time.timeScale = slowMotionRate;
        }
        else if (isButtonPressed && isSlowMotion)
        {
            isSlowMotion = false;
            Time.timeScale = currentTimeScale;
        }
    }

    private void AwakeModes()
    {
        if (modes.Length == 0)
        {
            throw new Exception("Modes is null, please assign nodes to the controller's array");
        }
        //Awake each of our modes
        foreach (var mode in modes)
        {
            mode.transform.SetParent(null);
            mode.AwakeMode(this);
        }
        //Set our initial mode to the first entry in the array
        SetCurrentMode(startingMode);
    }
    private void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        var robot = new RobotState(this);
        var car = new CarState(this);
        var transforming = new TransformingState(this);

        //begin transforming from any mode state once input is/was pressed and isn't locked
        At(robot,transforming, new FuncPredicate(() => (transformWasPressed)  && !transformInputLocked));
        At(car,transforming, new FuncPredicate(() => (transformWasPressed)  && !transformInputLocked));
        //transform into robot when we are a car and vice versa 
        At(transforming, robot, new FuncPredicate(() => transformTimer.IsFinished && currentMode is CarMode));
        At(transforming, car, new FuncPredicate(() => transformTimer.IsFinished && currentMode is RobotMode));

        //Set starting state to starting mode
        switch (GetCurrentMode())
        {
            case CarMode:
                stateMachine.SetState(car);
                break;
            case RobotMode:
                stateMachine.SetState(robot);
                break;
        }
    }
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    void HandleTransformInput(bool isButtonPressed)
    {
        //print("Jump Event!");
        if (!transformIsPressed && isButtonPressed)
        {
            transformWasPressed = true;
        }

        if (transformIsPressed && !isButtonPressed)
        {
            transformLetGo = true;
        }

        transformIsPressed = isButtonPressed;

    }
    void ResetTransformKeys()
    {
        transformLetGo = false;
        transformWasPressed = false;
    }
    public void OnTransformStart()
    {  
        transformInputLocked = true;
        transformTimer.Start();
        
        if (currentMode is RobotMode)
        {
            currentMode.TransformFrom(currentMode);
            GetModeOfType<CarMode>().TransformTo(currentMode);
            
            OnTransform.Invoke(ModeType.Robot,ModeType.Car);
        }
        else
        {
            currentMode.TransformFrom(currentMode);
            GetModeOfType<RobotMode>().TransformTo(currentMode);
            
            OnTransform.Invoke(ModeType.Car,ModeType.Robot);
        }

    }
    public void OnTransformExit()
    {
        previousMode = currentMode;
        previousMode.ExitMode();
        transformCooldowmTimer.Start();
    }
    private void HandleTransformCooldown() => transformInputLocked = false;

    public void OnModeStart<T>() where T : BaseMode
    {
        SetCurrentMode(GetModeOfType<T>());
       
        //if we have a previous mode, enter new mode with previous momentum, else just enter normally 
        if (previousMode != null) currentMode.EnterMode(previousMode.GetVelocity());
        else currentMode.EnterMode(Vector3.zero);
        
    }
    public void OnModeExit<T>() where T : BaseMode
    {
        //noop
    }
    
    void Update()
    {
        stateMachine.Update();
        tr.position = currentMode.transform.position;
        foreach (var mode in modes)
        {
            //update inactive mode position(s) to current mode position
            if (mode != currentMode)
            {
                mode.SetPosition(currentMode.transform.position);
            }
        }
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
        ResetTransformKeys();
    }

    void SetCurrentMode(BaseMode newMode)
    {
        currentMode = newMode;
        switch (currentMode)
        {
            case CarMode:
                currentModeType = ModeType.Car;
                break;
            case RobotMode:
                currentModeType = ModeType.Robot;
                break;
        }
        
    } 
    void SetCurrentMode(ModeType newModeType)
    {
        currentModeType = newModeType;
        switch (currentModeType)
        {
            case ModeType.Car:
                currentMode = GetModeOfType<CarMode>();
                break;
            case ModeType.Robot:
                currentMode = GetModeOfType<RobotMode>();
                break;
        }
        
    } 

    BaseMode GetModeOfType<T>() where T : BaseMode
    {
        foreach (var mode in modes)
        {
            if (mode is T) return mode;
        }
        throw new Exception("Could not find mode of Type T in controller mode array");
    }
}
