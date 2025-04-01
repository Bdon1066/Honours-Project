using System;
using System.Net.Http.Headers;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    enum CameraState{Free,Follow}
    CameraState cameraState = CameraState.Follow;
    
    float currentXAngle;
    float currentYAngle;

    [Range(0f, 90f)] public float upperVerticalLimit = 35f;
    [Range(0f, 90f)] public float lowerVerticalLimit = 35f;

    public float cameraSpeed = 90f;
    public bool invertYInput;
    public bool smoothCameraRotation;
    [Range(0f, 90f)] public float cameraSmoothingFactor = 25f;

    Transform tr;
    Camera cam;
    [SerializeField] InputReader input;
    [SerializeField] PlayerController player;
    private Vector3 cameraVelocity;

    private void Awake()
    {
        tr = transform;
        cam = GetComponentInChildren<Camera>();

        currentXAngle = tr.localRotation.eulerAngles.x;
        currentYAngle = tr.localRotation.eulerAngles.y;
        player.OnTransform += HandleTransform;
    }

    private void HandleTransform(ModeType fromMode, ModeType toMode)
    {
        switch (toMode)
        {
            case ModeType.Robot:
                cameraState = CameraState.Free;
                break;
            case ModeType.Car:
                cameraState = CameraState.Follow;
                break;
        }
    }

    private void Update()
    { 
        var YInverter = invertYInput ? -1f : 1f; //Invert our Y direction if we invertYInput is true
        switch (cameraState)
        {
            case CameraState.Free:
                FreeRotate(input.LookDirection.x, input.LookDirection.y * YInverter);
                break;
            case CameraState.Follow:
                FollowRotate();
                break;
        }
    }

    private void FollowRotate()
    {
       var modeTransform = player.GetCurrentMode().gameObject.transform;
       
       tr.localRotation = Quaternion.Euler(modeTransform.eulerAngles.x, modeTransform.eulerAngles.y, 0);
    }

    private void FreeRotate(float horizontalInput, float verticalInput)
    {
        if (smoothCameraRotation)
        {
            horizontalInput = Mathf.Lerp(0, horizontalInput, Time.deltaTime * cameraSmoothingFactor);
            verticalInput = Mathf.Lerp(0, verticalInput, Time.deltaTime * cameraSmoothingFactor);
        }

        currentXAngle += verticalInput * cameraSpeed * Time.deltaTime;
        currentYAngle += horizontalInput * cameraSpeed * Time.deltaTime;

        currentXAngle = Mathf.Clamp(currentXAngle,-upperVerticalLimit,lowerVerticalLimit);

        tr.localRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
    }
}

