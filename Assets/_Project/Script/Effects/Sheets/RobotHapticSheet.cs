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
    
}
