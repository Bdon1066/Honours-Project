using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class CarAudioPlayer : MonoBehaviour
{
    public CarMode carMode;
    public CollisionReader carCollision;
    
    
    public CarAudioSheet carAudio;

    public EventInstance Engine;
    public EventInstance Impact;

    public float minImpactSpeed = 1f;
    public float maxImpactSpeed = 30f;

    private bool playAudio;
    void Awake()
    {
        carMode.ToRobot += HandleTransformToRobot;
        carMode.ToCar += HandleTransformToCar;
        carMode.OnEnter += HandleCarEnter;

        carCollision.OnCollision += HandleCollison;
       
    }
    

    private void HandleCollison(Collision other)
    {
        if (!playAudio) return;
        
        var impactSpeed = other.relativeVelocity.magnitude;
        
        //if impact velocity lower than min, return and do not play
        if (other.relativeVelocity.magnitude < minImpactSpeed) return;
        
        float normalizedImpactSpeed = Mathf.Clamp01(Mathf.Abs(impactSpeed) / maxImpactSpeed);
        
        Impact = RuntimeManager.CreateInstance(carAudio.impact);
        //set impact to trigger at contact point
        Impact.set3DAttributes(other.GetContact(0).point.To3DAttributes());
        
        Impact.setParameterByName("Speed", normalizedImpactSpeed);

        Impact.start();
        print("CAR Audio!");

        //PlayOneShot(carAudio.impact,other.GetContact(0).point);
    }

    private void HandleCarEnter()
    {
        playAudio = true;
        
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
        carMode.OnLand += HandleLand;
    }
    private void HandleLand(Vector3 velocity)
    {
        PlayOneShot(carAudio.land, gameObject);
        print("CAR LAND Audio!");
    }

    private void HandleTransformToRobot()
    {
        PlayOneShot(carAudio.transform, carMode.gameObject);

        PLAYBACK_STATE currentState;
        Engine.getPlaybackState(out currentState);
        if (currentState == PLAYBACK_STATE.PLAYING)
        {
            Engine.stop(STOP_MODE.ALLOWFADEOUT);
            Engine.release();
        }
        carMode.OnLand -= HandleLand;
        playAudio = false;
    }

    void Update()
    {
        if (!playAudio) return;
        if (carMode.IsGrounded())
        {
            Engine.setParameterByName("Acceleration", carMode.normalizedSpeed);
        }
        
    }

    void PlayOneShot(EventReference eventRef,GameObject gameObject)
    {
        if (!playAudio) return;
        RuntimeManager.PlayOneShotAttached(eventRef,gameObject);
    }
    void PlayOneShot(EventReference eventRef,Vector3 position)
    {
        if (!playAudio) return;
        RuntimeManager.PlayOneShot(eventRef,position);
    }

}