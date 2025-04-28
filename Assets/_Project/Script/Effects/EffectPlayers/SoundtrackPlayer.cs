using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundtrackPlayer : MonoBehaviour
{
    public EventReference splashScreenTrack;
    public EventReference gameTrack;
    
    EventInstance SplashScreen;
    EventInstance Game;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        switch (scene.buildIndex)
        {
            case 0:
                break;
            case 1:
                if (IsPlaying(Game))
                {
                    Game.stop(STOP_MODE.ALLOWFADEOUT);
                    Game.release();
                }
                SplashScreen = RuntimeManager.CreateInstance(splashScreenTrack);
                SplashScreen.start();
                break;
            case 2:
                if (IsPlaying(SplashScreen))
                {
                    SplashScreen.stop(STOP_MODE.ALLOWFADEOUT);
                    SplashScreen.release();
                }
                Game = RuntimeManager.CreateInstance(gameTrack);
                Game.start();
                break;
        }
    }
    bool IsPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;   
        instance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }
    
}
