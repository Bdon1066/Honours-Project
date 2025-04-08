using FMOD.Studio;
using UnityEngine;

public class CarAudioPlayer : MonoBehaviour
{
    public CarMode carMode;
    
    public CarAudioSheet carAudio;
    
    void Awake()
    {
        //carMode.OnJump += HandleJump;
        //carMode.OnLand += HandleLand;
        carMode.ToRobot += HandleTransformToRobot;
        

    }

    private void HandleTransformToRobot()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(carAudio.transform, carMode.gameObject);
    }


}