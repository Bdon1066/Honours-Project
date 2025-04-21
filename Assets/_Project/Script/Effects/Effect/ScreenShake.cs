using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Effect/Screenshake")]
public class ScreenShake : ScriptableObject
{
    [Range(0, 1)] public float shakeTrauma = 0.5f;
}
