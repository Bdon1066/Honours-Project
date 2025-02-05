using UnityEngine;

public class CarMode : MonoBehaviour, IMode, IMovementStateController
{
    public GameObject model;
    CarMover mover;
    Transform tr;
    
    InputReader input;
    
    bool isEnabled; 
    
    public void Init(InputReader inputReader)
    {
        tr = transform;
        input = inputReader;
       
        mover = GetComponent<CarMover>();
        mover.Init();

    }
    public void ShowModel() => model.SetActive(true);
    public void HideModel() => model.SetActive(false);
    public Vector3 GetMomentum()
    {
       return Vector3.zero;
    }
    public IState GetState()
    {
        return null;
    }
    public void SetEnabled(bool value)
    {
        isEnabled = value;
    } 
    public bool IsEnabled() => isEnabled;
    public void EnterMode(IState entryState, Vector3 entryMomentum)
    {
        //stateMachine.SetState(entryState);
       // momentum = entryMomentum;
        
        mover.SetEnabled(true);
    }
    public void EnterMode()
    {
        mover.SetEnabled(true);
    }
    public void ExitMode()
    {
        mover.SetEnabled(false);
    }
}
