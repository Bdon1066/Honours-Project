using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Effect Sheet/Car Audio Sheet", fileName = "New Car Audio Sheet")] 
public class CarAudioSheet : ScriptableObject
{
    public EventReference engine;
    
    public EventReference transform;

    public EventReference land;
    
    public EventReference impact;
}
