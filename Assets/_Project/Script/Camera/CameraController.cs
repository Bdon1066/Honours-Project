using System;
using System.Net.Http.Headers;
using UnityEngine;

public class CameraController : MonoBehaviour
{
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

    private void Awake()
    {
        tr = transform;
        cam = GetComponentInChildren<Camera>();

        currentXAngle = tr.localRotation.eulerAngles.x;
        currentYAngle = tr.localRotation.eulerAngles.y;
    }

    private void Update()
    { 
        var YInverter = invertYInput ? -1f : 1f; //Invert our Y direction if we invertYInput is true
        
        RotateCamera(input.LookDirection.x, input.LookDirection.y * YInverter);
    }

    private void RotateCamera(float horizontalInput, float verticalInput)
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
