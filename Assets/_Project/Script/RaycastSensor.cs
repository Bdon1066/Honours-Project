using UnityEngine;



/// <summary>
/// A generic raycast script for handling things like a ground check.
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
    public RaycastSensor(Transform playerTransform)
    {
        tr = playerTransform;
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
}

