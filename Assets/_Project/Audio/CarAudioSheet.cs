using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Car Audio Sheet", fileName = "New Car Audio Sheet")] 
public class CarAudioSheet : ScriptableObject
{
    public EventReference engine;
    
    public EventReference transform;
}
