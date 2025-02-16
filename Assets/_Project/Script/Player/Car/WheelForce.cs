using UnityEngine;

public class WheelRay
{
    public Transform tr;
    public RaycastSensor suspensionRay;
    

    public WheelRay(Transform wheelTransform,GameObject carObject)
    {
        tr = wheelTransform;
        suspensionRay = new RaycastSensor(wheelTransform);

        //Recaculate wheel force rays to include this game objects layer collision stuff
        RecalculateSensorLayerMask(suspensionRay, carObject);
    }
    
    public bool IsGrounded() =>  suspensionRay.HasDetectedHit();
    
    public void RecalculateSensorLayerMask(RaycastSensor sensor, GameObject thisObject)
    {
        int objectLayer = thisObject.layer;
        int layerMask = Physics.AllLayers;

        for (int i = 0; i < 32; i++) //iterate through all layers
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i)) //check if layer i ignores this mover's layer
            {
                layerMask &= ~(1 << i); //remove layer i from the mask via magical bitshifting
            }
        }

        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast"); // and do the same with ignore raycast layer
        layerMask &= ~(1 << ignoreRaycastLayer);

        sensor.layerMask = layerMask;
    }
}
