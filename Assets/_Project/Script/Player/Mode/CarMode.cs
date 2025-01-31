using UnityEngine;

public class CarMode : MonoBehaviour, IMode , IMovementStateController
{
    public Vector3 GetMomentum()
    {
        return Vector3.zero;
    }

    public void Init(InputReader inputReader)
    {

    }


}