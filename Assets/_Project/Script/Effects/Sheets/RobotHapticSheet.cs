using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect Sheet/RobotHapticSheet")]
public class RobotHapticSheet : ScriptableObject
{
    public HapticEffect lightLandEffect;
    public HapticEffect mediumLandEffect;
    public HapticEffect heavyLandEffect;
    public HapticEffect footstepEffect;
    public HapticEffect wallstepEffect;
    public HapticEffect transform;

    [HideInInspector]public List<HapticEffect> activeEffects = new List<HapticEffect>();

    public void Init()
    {
        lightLandEffect.Init();
        mediumLandEffect.Init();
        heavyLandEffect.Init();
        footstepEffect.Init();
        wallstepEffect.Init();
        transform.Init();

        activeEffects.Add(heavyLandEffect);
        activeEffects.Add(mediumLandEffect);
        activeEffects.Add(lightLandEffect);
        activeEffects.Add(footstepEffect);
        activeEffects.Add(transform);
        activeEffects.Add(wallstepEffect);
    }
}
