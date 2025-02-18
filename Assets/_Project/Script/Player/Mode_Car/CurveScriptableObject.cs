using UnityEngine;

[CreateAssetMenu(fileName = "CurveScriptableObject")]
public class CurveScriptableObject : ScriptableObject
{
    public AnimationCurve curve;
    
    public float Evaluate(float time)
    {
        return curve.Evaluate(time);
    }
}


