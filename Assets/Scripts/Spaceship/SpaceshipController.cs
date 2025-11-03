using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    protected Rigidbody rb;
    
    [SerializeField] private float movementSpeed = 20;
    [SerializeField] private float torqueForce = 0.3f;
    [SerializeField] private float rollTorqueForce = 2;
    [SerializeField] private float verticalMovementSpeed = 10;
    [SerializeField] protected bool counteractRigidbodyMass = true;
    
    protected float MovementSpeed => movementSpeed;
    protected float VerticalMovementSpeed => verticalMovementSpeed;
    
    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    protected void Move(float forwardMovementSpeed, float forwardMovement = 1, float verticalMovement = 0, float forwardMovementMultiplier = 1)
    {
        float forceMultiplier = counteractRigidbodyMass ? rb.mass : 1f;
        
        rb.AddForce(transform.forward * (forwardMovement * forwardMovementSpeed * forceMultiplier * forwardMovementMultiplier), ForceMode.Force);
        rb.AddForce(transform.up * (verticalMovement * verticalMovementSpeed * forceMultiplier), ForceMode.Force);
    }

    protected void Rotate(Vector3 pitch, Vector3 yaw, Vector3 roll)
    {
        pitch *= torqueForce;
        yaw *= torqueForce;
        roll *= rollTorqueForce;
        
        Rotate_Internal(pitch + yaw + roll);
    }

    protected void Rotate(Vector3 torque)
    {
        Rotate_Internal(torque * torqueForce);
    }

    private void Rotate_Internal(Vector3 finalTorque)
    {
        float forceMultiplier = counteractRigidbodyMass ? rb.mass : 1f;
        rb.AddTorque(finalTorque * forceMultiplier, ForceMode.Force);
    }
}
