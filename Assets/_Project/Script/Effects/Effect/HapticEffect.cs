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
    
    public enum Type {Oneshot,Loop}
}

