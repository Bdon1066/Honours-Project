using System;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;


/// <summary>
/// This class creates a sensor using a raycast in a specified cast direction
/// </summary>
public class RaycastSensor
{
    public float castLength = 1.0f;
    public LayerMask layerMask = 255; //using numbers for layers is easier

    Vector3 origin = Vector3.zero;
    Transform tr;

    public enum CastDirection {Forward, Right, Up, Backward, Left, Down }
    CastDirection castDirection;
    
    RaycastHit hitInfo;

    // constructor to initialize class with player Transform
    public RaycastSensor(Transform originTransform)
    {
        tr = originTransform;
    }
    public RaycastSensor(GameObject originGameObject)
    {
        tr = originGameObject.transform;
        RecalculateSensorLayerMask(originGameObject);
    }
    public void Cast()
    {
        //We need world origin and direction as opposed to local
        Vector3 worldOrigin = tr.TransformPoint(origin);
        Vector3 worldDirection = GetCastDirection();

        Physics.Raycast(worldOrigin, worldDirection, out hitInfo, castLength, layerMask,QueryTriggerInteraction.Ignore);
        
    }
    
    public bool HasDetectedHit() => hitInfo.collider != null;
    public float GetDistance() => hitInfo.distance;
    public Vector3 GetNormal() => hitInfo.normal;
    public Vector3 GetPosition() => hitInfo.point;
    public Collider GetCollider() => hitInfo.collider;
    public Transform GetTransform() => hitInfo.transform;
    
    public void SetCastDirection(CastDirection direction) => castDirection = direction;
    public void SetCastOrigin(Vector3 pos) => origin = tr.InverseTransformPoint(pos);
    
    //Gets the actual vector direction from the corresponding enum
    Vector3 GetCastDirection()
    {
        return castDirection switch
        {
            CastDirection.Forward => tr.forward,
            CastDirection.Right => tr.right,
            CastDirection.Up => tr.up,
            CastDirection.Backward => -tr.forward,
            CastDirection.Left => -tr.right,
            CastDirection.Down => -tr.up,
            _ => Vector3.one
        };
    }
    public void RecalculateSensorLayerMask(GameObject thisObject)
    {
        int objectLayer = thisObject.layer;
        
        for (int i = 0; i < 32; i++) //iterate through all layers
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i)) //check if layer i ignores this mover's layer
            {
                layerMask &= ~(1 << i); //remove layer i from the mask via magical bitshifting
            }
        }
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast"); // and do the same with ignore raycast layer
        layerMask &= ~(1 << ignoreRaycastLayer);
    }
    public void DrawDebug()
    {
        if (!HasDetectedHit()) return;

        Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, Time.deltaTime);
        float markerSize = 0.2f;
        Debug.DrawLine(hitInfo.point + Vector3.up * markerSize, hitInfo.point - Vector3.up * markerSize, Color.green, Time.deltaTime);
        Debug.DrawLine(hitInfo.point + Vector3.right * markerSize, hitInfo.point - Vector3.right * markerSize, Color.green, Time.deltaTime);
        Debug.DrawLine(hitInfo.point + Vector3.forward * markerSize, hitInfo.point - Vector3.forward * markerSize, Color.green, Time.deltaTime);
    }
}

