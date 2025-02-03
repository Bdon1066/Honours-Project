using UnityEngine;

public class CarMode : MonoBehaviour, IMode , IMovementStateController
{
    public Vector3 GetMomentum()
    {
        return Vector3.zero;
    }
    [SerializeField] private GameObject model;

    public void Init(InputReader inputReader)
    {
    }

    public void ShowModel() => model.SetActive(true);
    public void HideModel() => model.SetActive(false);


}