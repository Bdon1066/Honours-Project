using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using static PlayerInputActions;


public interface IInputReader
{
    Vector2 Direction { get; }
    public void EnablePlayerActions();
}

[CreateAssetMenu(fileName = "PlayerInputReader")]
public class InputReader : ScriptableObject, IInputReader, PlayerInputActions.IPlayerActions
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<Vector2, bool> Look = delegate { };
    public event UnityAction<bool> Jump = delegate { };

    public event UnityAction<bool> Brake = delegate { };
    public event UnityAction<bool> Accelerate = delegate { };

    public event UnityAction<bool> Transform = delegate { };

    public event UnityAction<bool> SlowMotion = delegate { };
    public event UnityAction<bool> Pause = delegate { };

    public PlayerInputActions inputActions;

    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => inputActions.Player.Look.ReadValue<Vector2>();
    public bool isJumpPressed => inputActions.Player.Jump.IsPressed();

    public bool IsBrakePressed => inputActions.Player.Brake.IsPressed();
    public bool IsAcceleratePressed => inputActions.Player.Accelerate.IsPressed();

    public void EnablePlayerActions()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.Invoke(context.ReadValue<Vector2>());
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Jump.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Jump.Invoke(false);
                break;
        }
        
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }
    bool IsDeviceMouse(InputAction.CallbackContext context)
    {
        // Debug.Log($"Device name: {context.control.device.name}");
        return context.control.device.name == "Mouse";
    }

    public void OnTransform(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Transform.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Transform.Invoke(false);
                break;
        }
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Brake.Invoke(true);
                break;
            case InputActionPhase.Performed:
                Brake.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Brake.Invoke(false);
                break;
        }


    }

    public void OnToggleSlowMo(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                SlowMotion.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                SlowMotion.Invoke(false);
                break;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Pause.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Pause.Invoke(false);
                break;
        }
    }

    public void OnAccelerate(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Accelerate.Invoke(true);
                break;
            case InputActionPhase.Performed:
                Accelerate.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Accelerate.Invoke(false);
                break;
        }
    }
}

