using System.Collections;
using UnityEngine;
using ImprovedTimers;

public class CameraController : MonoBehaviour
{
    enum CameraState{Free,Follow}
    CameraState cameraState = CameraState.Free;
    
    [Header("Camera")]
    public Transform cameraTargetTransform;
    Transform tr;
    Camera cam;
    [SerializeField] InputReader input;
    [SerializeField] PlayerController player;
    
    [Range(0f, 90f)] public float upperVerticalLimit = 35f;
    [Range(0f, 90f)] public float lowerVerticalLimit = 35f;
    float currentXAngle;
    float currentYAngle;
    
    [Header("Follow Camera")]
    public Transform followTransform;
    private Camera followCam;
    public float followCamDelay = 0.25f;
    public float followCamDirectionOffset = 10f;
    public float minFOV;
    public float maxFOV;
    public CarMode carMode;
    private bool adjustFov;
    float targetFOV;
    
    [Header("Free Camera")]
    public Transform freeTransform;
    private Camera freeCam;
    public float freeCamDelay = 0.25f;
    public float cameraSpeed = 90f;
    public bool invertYInput;
    public bool smoothCameraRotation;
    [Range(0f, 90f)] public float cameraSmoothingFactor = 25f;
    

    private Vector3 transitionVelocity;

    private void Awake()
    {
        tr = transform;
        cam = cameraTargetTransform.GetComponentInChildren<Camera>();
        followCam = followTransform.GetComponentInChildren<Camera>();
        freeCam = freeTransform.GetComponentInChildren<Camera>();
        
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
                StartCoroutine(SwitchCamera(freeTransform,freeCam));
               // tr.localPosition = new Vector3(0, tr.localPosition.y, tr.localPosition.z);
                break;
            case ModeType.Car:
                cameraState = CameraState.Follow;
                StartCoroutine(SwitchCamera(followTransform,followCam));
                //tr.localPosition = new Vector3(tr.localPosition.x + 1, tr.localPosition.y, tr.localPosition.z);
                break;
        }
    }


    private void FixedUpdate()
    { 
        var YInverter = invertYInput ? -1f : 1f; //Invert our Y direction if invertYInput is true
        
        switch (cameraState)
        {
            case CameraState.Free:
                FreeRotate(input.LookDirection.x, input.LookDirection.y * YInverter);
               
                break;
            case CameraState.Follow:
                FollowRotate();
                HandleFOV();
                break;
        }
    }

    void HandleFOV()
    {
        if (!adjustFov) return;
        targetFOV = Mathf.Lerp(minFOV, maxFOV,  carMode.normalizedSpeed);
        
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 0.1f);
       
    }

    private void FollowRotate()
    {
       var modeTransform = player.GetCurrentMode().gameObject.transform;

       Quaternion targetRotation =  Quaternion.Euler(0, modeTransform.localRotation.eulerAngles.y, 0);;
      // tr.localRotation = Quaternion.Euler(modeTransform.eulerAngles.x, modeTransform.eulerAngles.y, 0);
       tr.localRotation = Quaternion.Slerp(tr.localRotation, targetRotation, followCamDelay);

        
       
       currentXAngle = tr.localRotation.eulerAngles.x;
       currentYAngle = tr.localRotation.eulerAngles.y;

       
    }

    private void FreeRotate(float horizontalInput, float verticalInput)
    {
        if (smoothCameraRotation)
        {
            horizontalInput = Mathf.Lerp(0, horizontalInput, Time.fixedDeltaTime* cameraSmoothingFactor);
            verticalInput = Mathf.Lerp(0, verticalInput, Time.fixedDeltaTime * cameraSmoothingFactor);
        }

        currentXAngle += verticalInput * cameraSpeed * Time.fixedDeltaTime;
        currentYAngle += horizontalInput * cameraSpeed * Time.fixedDeltaTime;

        currentXAngle = Mathf.Clamp(currentXAngle,-upperVerticalLimit,lowerVerticalLimit);

        Quaternion newRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
        tr.localRotation = Quaternion.Slerp(tr.localRotation, newRotation, freeCamDelay);
    }
    
    //https://discussions.unity.com/t/how-to-lerp-between-cameras-on-a-ui-button-click/55842
    IEnumerator SwitchCamera(Transform targetTransform, Camera targetCamera)
    {
        adjustFov = false;
        var animSpeed = 1.33f;
        
        Vector3 pos = cameraTargetTransform.localPosition;
        Quaternion rot = cameraTargetTransform.localRotation;
        float fov = cam.fieldOfView;
        
        float progress = 0.0f;

        while (progress < 1f)
        {
            cameraTargetTransform.localPosition = Vector3.Lerp(pos, targetTransform.localPosition, progress);
            cameraTargetTransform.localRotation = Quaternion.Lerp(rot, targetTransform.localRotation, progress);
            cam.fieldOfView = Mathf.Lerp(fov, targetCamera.fieldOfView, progress);
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime * animSpeed;
        }

        //Set final transform
        cameraTargetTransform.localPosition = targetTransform.transform.localPosition;
        cameraTargetTransform.localRotation = targetTransform.transform.localRotation;
        cam.fieldOfView = targetCamera.fieldOfView;
        
        adjustFov = true;
    }

}

