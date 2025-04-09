using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Robot Audio Sheet", fileName = "New Robot Audio Sheet")] 
public class RobotAudioSheet : ScriptableObject
{
    public EventReference footsteps;
    public EventReference jump;
    public EventReference land;
    public EventReference transform;
    
}
