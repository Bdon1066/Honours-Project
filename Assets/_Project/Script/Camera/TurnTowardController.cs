using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTowardController : MonoBehaviour
{
    [SerializeField] RobotMode robot;
    public float turnSpeed = 50f;

    Transform tr;
    float currentYRotation;
    const float fallOffAngle = 90f;

    void Start()
    {
        tr = transform;

        currentYRotation = tr.localEulerAngles.y;
    }

    void LateUpdate()
    {
        if (robot.isTransforming) return;
        Vector3 velocity = Vector3.ProjectOnPlane(robot.GetVelocity(), tr.up);
        if (velocity.magnitude < 0.001f) return;

        float angleDifference = Utils.GetAngleBetween(tr.forward, velocity.normalized, tr.up);

        float step = Mathf.Sign(angleDifference) *
                     Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(angleDifference)) *
                     Time.deltaTime * turnSpeed;

        currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDifference) ? angleDifference : step;

        tr.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }
}