using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class CarAudioPlayer : MonoBehaviour
{
    public CarMode carMode;
    
    public CarAudioSheet carAudio;

    public EventInstance Engine;
    void Awake()
    {
        //carMode.OnJump += HandleJump;
        //carMode.OnLand += HandleLand;
        carMode.ToRobot += HandleTransformToRobot;
        carMode.ToCar += HandleTransformToCar;
        carMode.OnEnter += HandleCarEnter;

    }

    private void HandleCarEnter()
    {
        PLAYBACK_STATE currentState;
        Engine.getPlaybackState(out currentState);
        if (currentState != PLAYBACK_STATE.PLAYING)
        {
            Engine = RuntimeManager.CreateInstance(carAudio.engine);
            RuntimeManager.AttachInstanceToGameObject(Engine, gameObject);
            Engine.start();
        }
    }

    private void HandleTransformToCar()
    {
       
    }

    private void HandleTransformToRobot()
    {
        RuntimeManager.PlayOneShotAttached(carAudio.transform, carMode.gameObject);

        PLAYBACK_STATE currentState;
        Engine.getPlaybackState(out currentState);
        if (currentState == PLAYBACK_STATE.PLAYING)
        {
            Engine.stop(STOP_MODE.ALLOWFADEOUT);
            Engine.release();
        }
    }

    void Update()
    {
        Engine.setParameterByName("Acceleration", carMode.normalizedSpeed);
    }

}