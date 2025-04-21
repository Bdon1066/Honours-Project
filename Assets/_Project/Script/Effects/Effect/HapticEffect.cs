using System;
using ImprovedTimers;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(menuName = "Effect/HapticEffect")]
public class HapticEffect : ScriptableObject
{
    public float lowSpeedIntesity = 1f;
    public AnimationCurve lowSpeedCurve;
    [Space]
    public float highSpeedIntesity = 1f;
    public AnimationCurve highSpeedCurve;
    [Space]
    public float duration = 0f;
    [Space]
    public Type type = Type.Oneshot;

    float lowSpeed;
    float highSpeed;
    public enum Type {Oneshot,Loop}

    CountdownTimer timer;

    public void Init()
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStop += OnTimerStop;
   
    }
    private void OnTimerStop()
    {
        if (type == Type.Oneshot)
        {
            Stop();
            return;
        }

        timer.Start();
    }
    public void Tick()
    {
       if (!timer.IsRunning) return;
       float progress = 1-(timer.CurrentTime / duration);
        
       lowSpeed = lowSpeedIntesity * lowSpeedCurve.Evaluate(progress);
       highSpeed = highSpeedIntesity*highSpeedCurve.Evaluate(progress);

       Gamepad.current.SetMotorSpeeds(lowSpeed, highSpeed);

    }
    public void Start()
    {
        timer.Start();
    }
    public void Stop()
    {
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

}

