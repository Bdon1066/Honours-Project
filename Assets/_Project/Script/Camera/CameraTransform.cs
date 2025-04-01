using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "CameraTransform", menuName = "CameraTransform")]
public class CameraTransform : ScriptableObject
{
    public Vector3 position;
    public Quaternion rotation;
}