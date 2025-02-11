using ImprovedTimers;
using UnityEngine;

[RequireComponent(typeof(CarMover))]
public class CarMode : BaseMode
{

    Transform tr;
    CarMover mover;
    InputReader input;
    PlayerController controller;
    
    public GameObject model;
    
    private bool isEnabled;
    public override void Init(PlayerController playerController)
    {  tr = transform; 
        controller = playerController;
        input = playerController.input;
      
        mover = GetComponent<CarMover>();
        mover.Init();
       
        //jumpTimer = new CountdownTimer(jumpDuration);
       // SetupStateMachine();
    }
    public override void EnterMode(Vector3 entryMomentum)
    {
        //momentum = entryMomentum;
        
        OnEnter();
    }
    public override void EnterMode() => OnEnter();
    void OnEnter()
    {
        SetEnabled(true);
        ShowModel();
        //input.Jump += HandleKeyJumpInput;
        mover.Enable();
        
    }
    public override void ExitMode() => OnExit();
    void OnExit()
    {
        //print("Exiting Robot Mode");
        SetEnabled(false);
        HideModel();
        //input.Jump -= HandleKeyJumpInput;
        mover.Disable();
    }
    public override void ShowModel() => model.SetActive(true);
    public override void HideModel() => model.SetActive(false);
    public override void SetEnabled(bool value) => isEnabled = value;
    public override bool IsEnabled() => isEnabled;
    public override Vector3 GetMomentum()
    {
        return Vector3.zero;
    }
    public override Vector3 GetMovementVelocity()
    {
       return Vector3.zero;
       
    }
   
}


