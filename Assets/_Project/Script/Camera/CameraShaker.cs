using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public RobotMode robot;
    public CarMode car;
    public CollisionReader carCollision;
    public GameObject shakyCamera;

    float trauma;

    float shakeFactor;

    public float maxXShake;
    public float maxYShake;
    public float maxZShake;

    public float shakeFallOffFactor = 0.5f;

    int Seed() => Random.Range(0, 10000);

    private void Start()
    {
        robot.OnLand += HandleLand;
        robot.OnWall += HandleWall;
        carCollision.OnCollision += HandleCollision;
    }

    private void HandleCollision(Collision collision)
    {
        if (!car.isEnabled) return;
        var impactSpeed = collision.relativeVelocity.magnitude;

        //if impact velocity lower than min, return and do not play
        if (collision.relativeVelocity.magnitude < 5) return;

        float normalizedImpactSpeed = Mathf.Clamp01(Mathf.Abs(impactSpeed) / 20);

        //Shake(normalizedImpactSpeed * factor);
      
    }

    private void HandleWall(Vector3 vector)
    {
       // Shake(0.4f);
    }

    private void HandleLand(LandForce force)
    {
        switch (force)
        {
            case LandForce.Heavy:
                Shake(0.7f);
                break;
            case LandForce.Medium:
                Shake(0.4f);
                break;
            case LandForce.Light:
                Shake(0.3f);
                break;
        }
    }

    void FixedUpdate()
    {
        if (trauma > 0)
        {
            trauma -= Time.deltaTime * shakeFallOffFactor;
            ShakeCamera();
        }

        
        if (trauma <= 0)
        {
            shakyCamera.transform.localRotation = Quaternion.identity;
        }

     
    }

    void ShakeCamera()
    {
        shakeFactor = Mathf.Pow(trauma,2);

        float xAngle = maxXShake * shakeFactor * Mathf.PerlinNoise(Seed(), Time.realtimeSinceStartup);
        float YAngle = maxYShake * shakeFactor * Mathf.PerlinNoise(Seed() + 1, Time.realtimeSinceStartup);
        float ZAngle = maxZShake * shakeFactor * Mathf.PerlinNoise(Seed() + 2, Time.realtimeSinceStartup);

        shakyCamera.transform.localRotation =  Quaternion.Euler(xAngle, YAngle,ZAngle);
    }

    public void Shake(float value)
    {
        value = Mathf.Clamp01(value);
        trauma += value;
        trauma = Mathf.Clamp01(trauma);

    }
   
}
