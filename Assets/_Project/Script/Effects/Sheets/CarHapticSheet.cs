using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarHapticSheet")]
public class CarHapticSheet : ScriptableObject
{
    public HapticEffect engine;
    public HapticEffect impact;
    public HapticEffect transform;
    public HapticEffect land;

    [HideInInspector]public List<HapticEffect> activeEffects = new List<HapticEffect>();

    public void Init()
    {
        engine.Init();
        impact.Init();
        transform.Init();
        land.Init();

        activeEffects.Add(engine);
        activeEffects.Add(impact);
        activeEffects.Add(transform);
        activeEffects.Add(land);
    }
}
