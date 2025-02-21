using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDistanceRaycaster : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform cameraTargetTransform;

    public LayerMask layerMask = Physics.AllLayers;
    public float minimumDistanceFromObstacles = 0.1f;
    public float smoothingFactor = 25f;

    Transform tr;
    float currentDistance;

    void Awake()
    {
        tr = transform;

        layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast")); //exclude ignore raycast layer from our layer mask
        layerMask &= ~(1 << LayerMask.NameToLayer("Car")); //exclude car layer from our layer mask
        layerMask &= ~(1 << LayerMask.NameToLayer("Robot")); //exclude robot layer from our layer mask
        currentDistance = (cameraTargetTransform.position - tr.position).magnitude;
    }

    void LateUpdate()
    {
        Vector3 castDirection = cameraTargetTransform.position - tr.position;

        float distance = GetCameraDistance(castDirection);

        currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothingFactor);
        cameraTransform.position = tr.position + castDirection.normalized * currentDistance;
    }

    float GetCameraDistance(Vector3 castDirection)
    {
        float distance = castDirection.magnitude + minimumDistanceFromObstacles;
        float sphereRadius = 0.5f;
        if (Physics.SphereCast(new Ray(tr.position, castDirection), sphereRadius, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacles);
        }
        return castDirection.magnitude;
    }
}