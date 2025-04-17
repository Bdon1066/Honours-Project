using System;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(fileName = "HapticEffect")]
public class HapticEffect : ScriptableObject
{
    public float lowSpeedIntesity = 1f;
    public float highSpeedIntesity = 1f;
    public float duration = 0f;

    CountdownTimer timer;


    public void Init()
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStop += OnTimerStop;

    }
    private void OnTimerStop()
    {
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    public void Start()
    {
        Gamepad.current.SetMotorSpeeds(lowSpeedIntesity, highSpeedIntesity);
        timer.Start();
    }


    
}
