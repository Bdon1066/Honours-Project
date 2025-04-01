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
    public float followCamDelay = 0.25f;
    public float followCamDirectionOffset = 10f;
    
    [Header("Free Camera")]
    public Transform freeTransform;
    public float freeCamDelay = 0.25f;
    public float cameraSpeed = 90f;
    public bool invertYInput;
    public bool smoothCameraRotation;
    [Range(0f, 90f)] public float cameraSmoothingFactor = 25f;
    

    private Vector3 transitionVelocity;

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
                StartCoroutine(SwitchCamera(cameraTargetTransform,freeTransform));
                break;
            case ModeType.Car:
                cameraState = CameraState.Follow;
                StartCoroutine(SwitchCamera(cameraTargetTransform,followTransform));
                break;
        }
    }


    private void FixedUpdate()
    { 
        var YInverter = invertYInput ? -1f : 1f; //Invert our Y direction if we invertYInput is true
        
        switch (cameraState)
        {
            case CameraState.Free:
                FreeRotate(input.LookDirection.x, input.LookDirection.y * YInverter);
                break;
            case CameraState.Follow:
                FollowRotate(input.LookDirection.x);
                break;
        }
    }

    private void FollowRotate(float horizontalInput)
    {
       var modeTransform = player.GetCurrentMode().gameObject.transform;

       var YOffset = followCamDirectionOffset * horizontalInput;
       print(YOffset);
       Quaternion targetRotation =  Quaternion.Euler(modeTransform.localRotation.eulerAngles.x, modeTransform.localRotation.eulerAngles.y + YOffset, 0);;
      // tr.localRotation = Quaternion.Euler(modeTransform.eulerAngles.x, modeTransform.eulerAngles.y, 0);
       tr.localRotation = Quaternion.Slerp(tr.localRotation, targetRotation, followCamDelay);
       
       currentXAngle = tr.localRotation.eulerAngles.x;
       currentYAngle = tr.localRotation.eulerAngles.y;
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

        Quaternion newRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
        tr.localRotation = Quaternion.Slerp(tr.localRotation, newRotation, freeCamDelay);
    }
    
    //https://discussions.unity.com/t/how-to-lerp-between-cameras-on-a-ui-button-click/55842
    IEnumerator SwitchCamera(Transform firstCamera, Transform secondCamera)
    {
        Camera firstCam = firstCamera.GetComponentInChildren<Camera>();
        Camera secondCam = secondCamera.GetComponentInChildren<Camera>();
        
        var animSpeed = 1f;
        
        Vector3 pos = firstCamera.localPosition;
        Quaternion rot = firstCamera.localRotation;
        float fov = firstCam.fieldOfView;
        
        

        float progress = 0.0f;  //This value is used for LERP

        while (progress < 0.75f)
        {
            firstCamera.localPosition = Vector3.Lerp(pos, secondCamera.transform.localPosition, progress);
            firstCamera.localRotation = Quaternion.Lerp(rot, secondCamera.transform.localRotation, progress);
            firstCam.fieldOfView = Mathf.Lerp(fov, secondCam.fieldOfView, progress);
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime * animSpeed;
        }

        //Set final transform
        firstCamera.localPosition = secondCamera.transform.localPosition;
        firstCamera.localRotation = secondCamera.transform.localRotation;
        firstCam.fieldOfView = secondCam.fieldOfView;
    }

}

