using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
    public event UnityAction<bool> Jump = delegate { };
    public event UnityAction<bool> Transform = delegate { };

    public PlayerInputActions inputActions;

    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public bool IsJumpPressed => inputActions.Player.Jump.IsPressed();
    public bool IsBraking => inputActions.Player.Brake.ReadValue<float>() > 0;

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
        throw new System.NotImplementedException();
    }
}

