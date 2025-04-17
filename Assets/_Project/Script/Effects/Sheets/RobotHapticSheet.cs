using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RobotHapticSheet")]
public class RobotHapticSheet : ScriptableObject
{
    public HapticEffect lightLandEffect;
    public HapticEffect mediumLandEffect;
    public HapticEffect heavyLandEffect;
    public HapticEffect footstepEffect;
    public HapticEffect transform;

    [HideInInspector]public List<HapticEffect> activeEffects = new List<HapticEffect>();

    public void Init()
    {
        lightLandEffect.Init();
        mediumLandEffect.Init();
        heavyLandEffect.Init();
        footstepEffect.Init();
        transform.Init();

        activeEffects.Add(heavyLandEffect);
        activeEffects.Add(mediumLandEffect);
        activeEffects.Add(lightLandEffect);
        activeEffects.Add(footstepEffect);
        activeEffects.Add(transform);
    }
}
