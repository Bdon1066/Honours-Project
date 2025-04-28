using System;
using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Timer = ImprovedTimers.Timer;

public class HapticManager : PersistentSingleton<HapticManager>
{
    private List<HapticInstance> hapticInstances = new List<HapticInstance>();


    private float totalLowSpeed;
    private float totalHighSpeed;

    public bool HasGamepad() => Gamepad.current != null;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        StopAllInstances();
    }
    HapticInstance CreateInstance(HapticEffect effect)
    {
        var hapticInstance = new HapticInstance(effect);
        hapticInstances.Add(hapticInstance);
        hapticInstance.Init();
        return hapticInstance;
    }
    /// <summary>
    /// Plays an effect and enforces it to be one shot.Useful if you just want to play the effect and forget about it.
    /// </summary>
    /// <param name="effect"></param>
    public void PlayOneShot(HapticEffect effect)
    {
        var instance = CreateInstance(effect);
        instance.Effect.type = HapticEffect.Type.Oneshot;
        instance.Start();
    }
    /// <summary>
    /// Begin Play of an effect. WARNING: You must manually Stop and Release this effect later.
    /// </summary>
    public HapticInstance Play(HapticEffect effect)
    {
        var instance = CreateInstance(effect);
        instance.Start();
        return instance;
    }

    void Update()
    {
        if (!HasGamepad()) return;
        totalLowSpeed = 0;
        totalHighSpeed = 0;

        for (int i = 0; i < hapticInstances.Count; i++)
        {
            if (hapticInstances[i].IsPlaying)
            {
                HandlePlaying(hapticInstances[i]);
            }

            if (hapticInstances[i].destroyFlag)
            {
                hapticInstances.RemoveAt(i);
                i--;
            }
        }
        
        
        
        
        Gamepad.current.SetMotorSpeeds(totalLowSpeed, totalHighSpeed);
    }
    private void HandlePlaying(HapticInstance instance)
    {
        if (!instance.Timer.IsRunning) return;
        float progress = 1-(instance.Timer.CurrentTime / instance.Effect.duration);
        
        var lowSpeed = instance.Effect.lowSpeedIntesity * instance.Effect.lowSpeedCurve.Evaluate(progress);
        var highSpeed = instance.Effect.highSpeedIntesity*instance.Effect.highSpeedCurve.Evaluate(progress);
        
        totalLowSpeed = Mathf.Clamp01(totalLowSpeed += lowSpeed);
        totalHighSpeed = Mathf.Clamp01(totalHighSpeed += highSpeed);
        
       
    }
    private void OnDestroy()
    {
        if (HasGamepad())
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }

    void DestroyAllInstances()
    {
        foreach (var instance in hapticInstances)
        {
            instance.destroyFlag = true;
        }
    }
    void StopAllInstances()
    {
        foreach (var instance in hapticInstances)
        {
            instance.Stop();
        }
    }

}

public class HapticInstance
{
    public HapticEffect Effect;
    
    public CountdownTimer Timer { get; private set; }
    
    public bool IsPlaying { get; private set; }

    public bool destroyFlag;

    public HapticInstance(HapticEffect effect)
    {
        this.Effect = effect;
    }
    public void Init()
    {
        Timer = new CountdownTimer(Effect.duration);
        Timer.OnTimerStop += OnDurationReached;
    }
    private void OnDurationReached()
    {
        if (Effect.type == HapticEffect.Type.Oneshot)
        {
            Release();
            return;
        }
        
        Timer.Start();
    }


    public void Start()
    {
        Timer.Start();
        IsPlaying = true;
    }
    
    public void Stop()
    {
        Timer.Stop();
        IsPlaying = false;
    }
    public void Release()
    {
        Stop();
        Timer.OnTimerStop -= OnDurationReached;
        destroyFlag = true;
    }
    
    
    
}
