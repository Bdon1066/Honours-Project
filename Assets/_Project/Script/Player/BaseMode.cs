using UnityEngine;

public  interface IMode 
{
    
}

public class RobotMode : MonoBehaviour, IMode
{
    #region Fields

    [SerializeField] private InputReader input;

    Transform tr;
    IMover mover;

    bool jumpInputLocked, jumpWasPressed, jumpLetGo, jumpIsPressed;

    public float movementSpeed = 7f;
    public float groundFriction = 100f;
    public float gravity = 30f;
    
    public float jumpSpeed = 10f;
    public float jumpDuration = 0.2f;
    public float airControlRate = 2f;
    public float airFriction = 0.5f;
   
    public float slideGravity = 5f;
    
    public bool useLocalMomentum;
    
    //StateMachine stateMachine;
    //CountdownTimer jumpTimer;

    #endregion
}