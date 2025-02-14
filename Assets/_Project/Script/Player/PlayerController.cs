using System;
using ImprovedTimers;
using UnityEngine;

public class PlayerController : MonoBehaviour, IModeStateController
{
    public InputReader input;
    public float transformDuration = 1f;
    StateMachine stateMachine;
    Transform tr;

    BaseMode currentMode;
    BaseMode previousMode;

    public BaseMode[] modes;

    bool transformInputLocked, transformWasPressed, transformLetGo, transformIsPressed;

    CountdownTimer transformTimer;

    public event Action<Vector3> OnTransform = delegate { };

    public BaseMode GetCurrentMode() => currentMode;
    BaseMode GetPreviousMode() => currentMode;
    void Awake()
    {
        tr = transform;
        transformTimer = new CountdownTimer(transformDuration);
        Cursor.lockState = CursorLockMode.Locked;
    }
 
    private void Start()
    {
        //initialize our modes on start and set up the mode state machine
        InitModes();
        SetupStateMachine();
        
        input.EnablePlayerActions();
        input.Transform += HandleTransformInput;
    }
    private void InitModes()
    {
        if (modes.Length == 0)
        {
            throw new Exception("Modes is null, please assign nodes to the controller's array");
        }
        
        //initialize each of our modes
        foreach (var mode in modes)
        {
            mode.transform.SetParent(null);
            mode.Init(this);
        }
        
        //Set our initial mode to the first entry in the array
        SetCurrentMode(modes[0]); 

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

        //Set starting state to starting mode
        switch (GetCurrentMode())
        {
            case CarMode:
                stateMachine.SetState(car);
                break;
            case RobotMode:
                stateMachine.SetState(robot);
                break;
            default:
                break;
        }
    }
    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

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
        print("TRANBSFROM");

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

        OnTransform.Invoke(currentMode.GetVelocity());
    }
    public void OnTransformExit()
    {
        previousMode = currentMode;
        previousMode.ExitMode();
    }

    public void OnModeStart<T>() where T : BaseMode
    {
        SetCurrentMode(GetModeOfType<T>());
       
        //if we have a previous mode, enter new mode with previous momentum, else just enter normally 
        if (previousMode != null) currentMode.EnterMode(previousMode.GetVelocity(), previousMode.GetDirection());
        else currentMode.EnterMode(Vector3.zero, Vector3.zero);

    }
    public void OnModeExit<T>() where T : BaseMode
    {
        //currentMode.ExitMode();
    }
    
    void Update()
    {
        stateMachine.Update();
        tr.position = currentMode.transform.position;
        foreach (var mode in modes)
        {
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
        
        
        
        //TODO: Refactor thjis so that its probably all done in the transform entry state since states go like this:
        //TODO: RobotExit -> TransformEnter -> TransformExit -> Car Enter
        //TODO: And we want the previous mode model to dissapear on TransformExit instead of in CarEnter
        
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
