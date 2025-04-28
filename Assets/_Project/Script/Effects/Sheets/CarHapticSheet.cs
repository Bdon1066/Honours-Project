using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect Sheet/CarHapticSheet")]
public class CarHapticSheet : ScriptableObject
{
    public HapticEffect engine;
    public HapticEffect impact;
    public HapticEffect transform;
    public HapticEffect land;
    
}
